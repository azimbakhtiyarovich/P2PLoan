using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.CreditScore;
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

    public async Task<CreditScoreResultDto> CalculateAndSaveAsync(Guid borrowerProfileId)
    {
        var profile = await _context.BorrowerProfiles
            .Include(bp => bp.Loans)
                .ThenInclude(l => l.Repayments)
            .FirstOrDefaultAsync(bp => bp.Id == borrowerProfileId)
            ?? throw new NotFoundException("BorrowerProfile", borrowerProfileId);

        var result = Compute(profile);

        // Natijani saqlash
        profile.CreditScore = result.Score;
        profile.CreditRating = result.Rating;
        profile.LastScoredAt = result.CalculatedAt;
        await _context.SaveChangesAsync();

        return result;
    }

    public async Task<CreditScoreResultDto?> GetLatestScoreAsync(Guid borrowerProfileId)
    {
        var profile = await _context.BorrowerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.Id == borrowerProfileId);

        if (profile?.CreditScore is null) return null;

        return new CreditScoreResultDto
        {
            BorrowerId = borrowerProfileId,
            Score = profile.CreditScore.Value,
            Rating = profile.CreditRating,
            IsEligible = profile.CreditScore >= MinEligibleScore,
            MinRequiredScore = MinEligibleScore,
            CalculatedAt = profile.LastScoredAt ?? DateTimeOffset.UtcNow
        };
    }

    public async Task EnsureEligibleAsync(Guid borrowerProfileId, decimal loanAmount)
    {
        var profile = await _context.BorrowerProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(bp => bp.Id == borrowerProfileId)
            ?? throw new NotFoundException("BorrowerProfile", borrowerProfileId);

        // Agar ball hali hisoblanmagan bo'lsa — yangilash
        if (profile.CreditScore is null || profile.LastScoredAt < DateTimeOffset.UtcNow.AddDays(-30))
            await CalculateAndSaveAsync(borrowerProfileId);

        // Re-fetch after possible recalc
        profile = await _context.BorrowerProfiles.AsNoTracking()
            .FirstAsync(bp => bp.Id == borrowerProfileId);

        var score = profile.CreditScore ?? 300;

        // Katta kredit uchun yuqoriroq ball talab qilinadi
        int required = loanAmount switch
        {
            <= 5_000_000m => MinEligibleScore,         // ≤5M UZS
            <= 20_000_000m => 550,                     // ≤20M UZS
            <= 50_000_000m => 600,                     // ≤50M UZS
            _ => 650                                    // >50M UZS
        };

        if (score < required)
            throw new CreditCheckFailedException(score, required);
    }

    // ─────────────────────────────────────────────────────────────────────────
    //  HISOBLASH LOGIKASI
    // ─────────────────────────────────────────────────────────────────────────

    private CreditScoreResultDto Compute(Core.Entities.BorrowerProfile profile)
    {
        var kyc    = ComputeKycFactor(profile.KycStatus);          // 0-100
        var income = ComputeIncomeFactor(profile.MonthlyIncome);   // 0-100
        var dti    = ComputeDtiFactor(profile.MonthlyIncome, profile.ExistingDebt); // 0-100
        var hist   = ComputeHistoryFactor(profile.Loans);          // 0-100

        // Weighted sum (0-100)
        var weighted = kyc * 0.15m + income * 0.30m + dti * 0.25m + hist * 0.30m;

        // Final score: 300-850
        var score = (int)(MinScore + weighted / 100m * ScoreRange);
        score = Math.Clamp(score, MinScore, MaxScore);

        return new CreditScoreResultDto
        {
            BorrowerId           = profile.Id,
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

    /// <summary>
    /// KYC holati bo'yicha 0-100 ball.
    /// Verified=100, Pending=40, Rejected/NotSubmitted=0
    /// </summary>
    private static decimal ComputeKycFactor(KycStatus status) => status switch
    {
        KycStatus.Verified     => 100m,
        KycStatus.Pending      => 40m,
        KycStatus.Rejected     => 0m,
        _                      => 0m    // NotSubmitted
    };

    /// <summary>
    /// Oylik daromad (UZS) bo'yicha 0-100 ball.
    /// </summary>
    private static decimal ComputeIncomeFactor(decimal monthlyIncome) => monthlyIncome switch
    {
        >= 10_000_000m => 100m,
        >= 5_000_000m  => 85m,
        >= 2_000_000m  => 65m,
        >= 1_000_000m  => 40m,
        > 0m           => 20m,
        _              => 0m
    };

    /// <summary>
    /// Debt-to-Income (yillik) nisbati bo'yicha 0-100 ball.
    /// DTI = ExistingDebt / (MonthlyIncome * 12)
    /// </summary>
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

    /// <summary>
    /// O'tmishdagi to'lov tarixi bo'yicha 0-100 ball.
    /// Yangi borrower (tarixi yo'q): neytral 60 ball.
    /// Har bir muddatida to'langan to'lov +, kechikkan -.
    /// </summary>
    private static decimal ComputeHistoryFactor(ICollection<Core.Entities.Loan> loans)
    {
        var allRepayments = loans
            .SelectMany(l => l.Repayments)
            .Where(r => r.Status == PaymentStatus.Success)
            .ToList();

        if (!allRepayments.Any()) return 60m; // Yangi borrower — neytral

        var total  = allRepayments.Count;
        var onTime = allRepayments.Count(r => r.PaidAt.HasValue && r.PaidAt.Value.Date <= r.DueDate);
        var ratio  = (decimal)onTime / total;

        // ratio [0..1] → [0..100], minimum 5 ta to'lov bo'lganda ishonchli
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
