using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Loan;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class LoanService : ILoanService
{
    private readonly ApplicationDbContext  _context;
    private readonly ICreditScoringService _creditScoring;
    private readonly INotificationService  _notifications;
    private readonly IAuditService         _audit;

    public LoanService(
        ApplicationDbContext context,
        ICreditScoringService creditScoring,
        INotificationService notifications,
        IAuditService audit)
    {
        _context       = context;
        _creditScoring = creditScoring;
        _notifications = notifications;
        _audit         = audit;
    }

    public async Task<Loan> CreateLoanAsync(CreateLoanDto dto, Guid borrowerProfileId)
    {
        // 1. Validatsiya BIRINCHI (credit check dan oldin — FIX #13)
        if (dto.Amount <= 0)
            throw new ValidationException("amount", "Kredit summasi musbat bo'lishi kerak.");
        if (dto.InterestRate is < 1 or > 100)
            throw new ValidationException("interestRate", "Foiz stavkasi 1-100% orasida bo'lishi kerak.");
        if (dto.DurationDays < 7)
            throw new ValidationException("durationDays", "Kredit muddati kamida 7 kun bo'lishi kerak.");
        if (dto.MinContribution.HasValue && dto.MinContribution.Value > dto.Amount)
            throw new ValidationException("minContribution", "Minimal investitsiya miqdori kredit summasidan oshmasligi kerak.");

        // 2. Credit check (validatsiyadan keyin)
        await _creditScoring.EnsureEligibleAsync(borrowerProfileId, dto.Amount);

        var profile = await _context.BorrowerProfiles
            .FirstOrDefaultAsync(bp => bp.Id == borrowerProfileId)
            ?? throw new NotFoundException("BorrowerProfile", borrowerProfileId);

        // 3. Loan yaratish
        var loan = new Loan
        {
            BorrowerId         = borrowerProfileId,
            Title              = dto.Title,
            Description        = dto.Description,
            Amount             = dto.Amount,
            MinContribution    = dto.MinContribution ?? 1m,
            DurationDays       = dto.DurationDays,
            InterestRate       = dto.InterestRate,
            RepaymentFrequency = dto.Frequency,
            Status             = LoanStatus.OpenForFunding,
            OpenUntil          = dto.OpenUntil ?? DateTimeOffset.UtcNow.AddDays(30)
        };

        _context.Loans.Add(loan);
        await _context.SaveChangesAsync();

        // 4. Audit log
        await _audit.LogAsync("Loan", loan.Id, "Created", profile.UserId,
            new { loan.Amount, loan.InterestRate, loan.DurationDays });

        // 5. Notification
        await _notifications.SendAsync(profile.UserId, "Kredit arizasi yaratildi",
            $"'{loan.Title}' nomli {loan.Amount:N0} UZS miqdordagi kredit arizangiz qabul qilindi.");

        return loan;
    }

    public async Task<Loan?> GetLoanByIdAsync(Guid id)
    {
        return await _context.Loans
            .Include(l => l.Borrower)
            .Include(l => l.Investments)
            .Include(l => l.Repayments)
            .Include(l => l.Offers)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<IEnumerable<Loan>> GetLoansByBorrowerAsync(Guid borrowerProfileId)
    {
        return await _context.Loans
            .AsNoTracking()
            .Where(l => l.BorrowerId == borrowerProfileId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Loan>> GetOpenLoansAsync(int page = 1, int pageSize = 20)
    {
        return await _context.Loans
            .AsNoTracking()
            .Where(l => l.Status == LoanStatus.OpenForFunding
                     || l.Status == LoanStatus.PartiallyFunded)
            .OrderBy(l => l.OpenUntil)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Include(l => l.Borrower)
            .AsSplitQuery()
            .ToListAsync();
    }

    public async Task<bool> UpdateLoanStatusAsync(
        Guid loanId, LoanStatus newStatus, Guid? performedBy = null)
    {
        var loan = await _context.Loans.FindAsync(loanId);
        if (loan is null) return false;

        ValidateStateTransition(loan.Status, newStatus);

        var oldStatus = loan.Status; // capture BEFORE changing — FIX #13
        loan.Status = newStatus;
        if (newStatus == LoanStatus.Active)
            loan.StartDate = DateTimeOffset.UtcNow;

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Loan", loanId, $"StatusChanged:{newStatus}", performedBy,
            new { From = oldStatus, To = newStatus });

        return true;
    }

    public async Task AcceptLoanAsync(Guid loanId, Guid borrowerUserId)
    {
        var loan = await _context.Loans
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == loanId)
            ?? throw new NotFoundException("Loan", loanId);

        if (loan.Borrower?.UserId != borrowerUserId)
            throw new UnauthorizedException("Bu kredit sizga tegishli emas.");

        if (loan.Status != LoanStatus.Funded)
            throw new InvalidLoanStateException(loan.Status, LoanStatus.Funded);

        loan.Status              = LoanStatus.Active;
        loan.AcceptedByBorrower  = true;
        loan.StartDate           = DateTimeOffset.UtcNow;

        // Repayment schedule generatsiya
        await GenerateRepaymentScheduleAsync(loan);

        await _context.SaveChangesAsync();
        await _audit.LogAsync("Loan", loanId, "AcceptedByBorrower", borrowerUserId, null);
        await _notifications.SendAsync(borrowerUserId, "Kreditingiz faollashtirildi",
            $"'{loan.Title}' nomli kreditingiz tasdiqlandi va to'lov jadvali tuzildi.");
    }

    public async Task<IEnumerable<Repayment>> GetRepaymentScheduleAsync(Guid loanId)
    {
        return await _context.Repayments
            .AsNoTracking()
            .Where(r => r.LoanId == loanId)
            .OrderBy(r => r.DueDate)
            .ToListAsync();
    }

    // ── Repayment Schedule Generator ─────────────────────────────────────────

    /// <summary>
    /// To'lov jadvalini hisoblaydi.
    /// Simple interest (oddiy foiz): har bir to'lovda teng qismda bo'linadi.
    /// Formula: InterestAmount = (Amount * Rate/100) / numInstallments
    ///          PrincipalAmount = Amount / numInstallments
    /// </summary>
    private async Task GenerateRepaymentScheduleAsync(Loan loan)
    {
        // Avvalgi schedule larni o'chirish (agar qayta qabul bo'lsa)
        var existing = await _context.Repayments
            .Where(r => r.LoanId == loan.Id && r.Status == PaymentStatus.Created)
            .ToListAsync();
        _context.Repayments.RemoveRange(existing);

        var numInstallments = GetInstallmentCount(loan.DurationDays, loan.RepaymentFrequency);
        var principal    = Math.Round(loan.Amount / numInstallments, 2);
        var interest     = Math.Round((loan.Amount * loan.InterestRate / 100m) / numInstallments, 2);
        var startDate    = loan.StartDate ?? DateTimeOffset.UtcNow;

        for (int i = 1; i <= numInstallments; i++)
        {
            var dueDate = CalculateDueDate(startDate, i, loan.RepaymentFrequency);

            // Oxirgi to'lovda yaxlitlash farqini to'liq hisoblaymiz
            var actualPrincipal = (i == numInstallments)
                ? loan.Amount - principal * (numInstallments - 1)
                : principal;

            _context.Repayments.Add(new Repayment
            {
                LoanId          = loan.Id,
                DueDate         = dueDate,
                PrincipalAmount = actualPrincipal,
                InterestAmount  = interest,
                Amount          = actualPrincipal + interest,
                Status          = PaymentStatus.Created
            });
        }
    }

    private static int GetInstallmentCount(int durationDays, RepaymentFrequency frequency)
    {
        return frequency switch
        {
            RepaymentFrequency.Daily   => durationDays,
            RepaymentFrequency.Weekly  => Math.Max(1, durationDays / 7),
            RepaymentFrequency.Monthly => Math.Max(1, durationDays / 30),
            RepaymentFrequency.Yearly  => Math.Max(1, durationDays / 365),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency))
        };
    }

    private static DateTimeOffset CalculateDueDate(
        DateTimeOffset start, int installmentNumber, RepaymentFrequency frequency)
    {
        return frequency switch
        {
            RepaymentFrequency.Daily   => start.AddDays(installmentNumber),
            RepaymentFrequency.Weekly  => start.AddDays(installmentNumber * 7),
            RepaymentFrequency.Monthly => start.AddMonths(installmentNumber),
            RepaymentFrequency.Yearly  => start.AddYears(installmentNumber),
            _ => throw new ArgumentOutOfRangeException(nameof(frequency))
        };
    }

    // ── State Machine ────────────────────────────────────────────────────────

    /// <summary>
    /// Loan holat o'zgarishining to'g'riligini tekshiradi.
    /// Faqat ruxsat etilgan o'tishlar qabul qilinadi.
    /// </summary>
    private static void ValidateStateTransition(LoanStatus from, LoanStatus to)
    {
        var allowed = from switch
        {
            LoanStatus.Created           => new[] { LoanStatus.OpenForFunding, LoanStatus.Cancelled },
            LoanStatus.OpenForFunding    => new[] { LoanStatus.PartiallyFunded, LoanStatus.Funded, LoanStatus.Cancelled },
            LoanStatus.PartiallyFunded   => new[] { LoanStatus.Funded, LoanStatus.Cancelled },
            LoanStatus.Funded            => new[] { LoanStatus.AcceptedByBorrower },
            LoanStatus.AcceptedByBorrower=> new[] { LoanStatus.Active },
            LoanStatus.Active            => new[] { LoanStatus.Repayment, LoanStatus.Overdue },
            LoanStatus.Repayment         => new[] { LoanStatus.Paid, LoanStatus.Overdue, LoanStatus.Default },
            LoanStatus.Overdue           => new[] { LoanStatus.Repayment, LoanStatus.Default },
            _                            => Array.Empty<LoanStatus>()
        };

        if (!allowed.Contains(to))
            throw new InvalidLoanStateException(
                $"'{from}' holatidan '{to}' holatiga o'tish mumkin emas.");
    }
}
