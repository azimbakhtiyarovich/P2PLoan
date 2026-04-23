using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.CreditScore;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

/// <summary>
/// Kredit ball hisoblash algoritmi.
///
/// Maksimal ball: 850. Minimal: 300.
/// Formula: Score = 300 + (weighted_sum / 100 * 550)
///
/// Faktorlar:
///   1. KYC holati        — 15% (max 82.5 pt)
///   2. Oylik daromad     — 30% (max 165 pt)
///   3. Debt-to-Income    — 25% (max 137.5 pt)
///   4. To'lov tarixi     — 30% (max 165 pt)
/// </summary>
public class CreditScoringService : ICreditScoringService
{
    private const int MinScore = 300;
    private const int MaxScore = 850;
    private const int ScoreRange = MaxScore - MinScore; // 550
    private const int MinEligibleScore = 500;

    private readonly ApplicationDbContext _context;

    public CreditScoringService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<CreditScoreResultDto> CalculateAndSaveAsync(Guid userId)
    {
        var profile = await _context.UserProfiles
            .Include(up => up.User)
                .ThenInclude(u => u!.Loans)
                    .ThenInclude(l => l.Repayments)
            .FirstOrDefaultAsync(up => up.UserId == userId)
            ?? throw new NotFoundException("UserProfile", userId);

        var result = Compute(profile);

        // Natijani saqlash
        profile.CreditScore = result.Score;
        profile.CreditRating = result.Rating;
        profile.LastScoredAt = result.CalculatedAt;
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<CreditScoreResultDto?> GetLatestScoreAsync(Guid userId)
    {
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId);

        if (profile?.CreditScore is null) return null;

        return new CreditScoreResultDto
        {
            UserId = userId,
            Score = profile.CreditScore.Value,
            Rating = profile.CreditRating,
            IsEligible = profile.CreditScore >= MinEligibleScore,
            MinRequiredScore = MinEligibleScore,
            CalculatedAt = profile.LastScoredAt ?? DateTimeOffset.UtcNow
        };
    }

    public async Task EnsureEligibleAsync(Guid userId, decimal loanAmount)
    {
        var profile = await _context.UserProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(up => up.UserId == userId)
            ?? throw new NotFoundException("UserProfile", userId);

        // Agar ball eskirgan yoki yo'q bo'lsa — yangilash
        var needsRecalc = profile.CreditScore is null
            || profile.LastScoredAt < DateTimeOffset.UtcNow.AddDays(-30);

        if (needsRecalc)
        {
            var result = await CalculateAndSaveAsync(userId);
            var score2    = result.Score;
            var required2 = GetRequiredScore(loanAmount);
            if (score2 < required2)
                throw new CreditCheckFailedException(score2, required2);
            return;
        }

        var score    = profile.CreditScore ?? 300;
        var required = GetRequiredScore(loanAmount);
        if (score < required)
            throw new CreditCheckFailedException(score, required);
    }

    private static int GetRequiredScore(decimal loanAmount) => loanAmount switch
    {
        <= 5_000_000m  => 500,
        <= 20_000_000m => 550,
        <= 50_000_000m => 600,
        _              => 650
    };

    // ─────────────────────────────────────────────────────────────────────────
    //  HISOBLASH LOGIKASI
    // ─────────────────────────────────────────────────────────────────────────

    private CreditScoreResultDto Compute(UserProfile profile)
    {
        var loans = profile.User?.Loans ?? new List<Loan>();

        var kyc    = ComputeKycFactor(profile.KycStatus);
        var income = ComputeIncomeFactor(profile.MonthlyIncome);
        var dti    = ComputeDtiFactor(profile.MonthlyIncome, profile.ExistingDebt);
        var hist   = ComputeHistoryFactor(loans);

        // Weighted sum (0-100)
        var weighted = kyc * 0.15m + income * 0.30m + dti * 0.25m + hist * 0.30m;

        // Final score: 300-850
        var score = (int)(MinScore + weighted / 100m * ScoreRange);
        score = Math.Clamp(score, MinScore, MaxScore);

        return new CreditScoreResultDto
        {
            UserId               = profile.UserId,
            Score                = score,
            Rating               = ScoreToRating(score),
            IsEligible           = score >= MinEligibleScore,
            MinRequiredScore     = MinEligibleScore,
            KycFactor            = kyc,
            IncomeFactor         = income,
            DebtToIncomeFactor   = dti,
            RepaymentHistoryFactor = hist,
            CalculatedAt         = DateTimeOffset.UtcNow
        };
    }

    private static decimal ComputeKycFactor(KycStatus status) => status switch
    {
        KycStatus.Verified     => 100m,
        KycStatus.Pending      => 40m,
        KycStatus.Rejected     => 0m,
        _                      => 0m
    };

    private static decimal ComputeIncomeFactor(decimal monthlyIncome) => monthlyIncome switch
    {
        >= 10_000_000m => 100m,
        >= 5_000_000m  => 85m,
        >= 2_000_000m  => 65m,
        >= 1_000_000m  => 40m,
        > 0m           => 20m,
        _              => 0m
    };

    private static decimal ComputeDtiFactor(decimal monthlyIncome, decimal? existingDebt)
    {
        if (monthlyIncome <= 0) return 0m;
        if (existingDebt is null or 0m) return 100m;

        var annualIncome = monthlyIncome * 12;
        var dti = existingDebt.Value / annualIncome;

        return dti switch
        {
            <= 0.10m => 100m,
            <= 0.20m => 80m,
            <= 0.40m => 55m,
            <= 0.60m => 30m,
            _        => 0m
        };
    }

    private static decimal ComputeHistoryFactor(ICollection<Loan> loans)
    {
        var allRepayments = loans
            .SelectMany(l => l.Repayments)
            .Where(r => r.Status == PaymentStatus.Success)
            .ToList();

        if (!allRepayments.Any()) return 60m;

        var total  = allRepayments.Count;
        var onTime = allRepayments.Count(r => r.PaidAt.HasValue && r.PaidAt.Value.Date <= r.DueDate);
        var ratio  = (decimal)onTime / total;

        var reliability = total >= 5 ? 1.0m : (decimal)total / 5;
        return ratio * 100m * (0.7m + 0.3m * reliability);
    }

    private static CreditRating ScoreToRating(int score) => score switch
    {
        >= 750 => CreditRating.AAA,
        >= 700 => CreditRating.AA,
        >= 650 => CreditRating.A,
        >= 600 => CreditRating.BBB,
        >= 550 => CreditRating.BB,
        >= 500 => CreditRating.B,
        _      => CreditRating.CCC
    };
}
