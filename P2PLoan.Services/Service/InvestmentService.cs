using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Investment;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class InvestmentService : IInvestmentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWalletService       _wallet;
    private readonly INotificationService _notifications;
    private readonly IAuditService        _audit;

    public InvestmentService(
        ApplicationDbContext context,
        IWalletService wallet,
        INotificationService notifications,
        IAuditService audit)
    {
        _context       = context;
        _wallet        = wallet;
        _notifications = notifications;
        _audit         = audit;
    }

    public async Task<Investment> InvestAsync(Guid lenderUserId, InvestDto dto)
    {
        // ── Validatsiya (DB ga bormasdan avval) ───────────────────────────
        var loan = await _context.Loans
            .Include(l => l.Borrower)
            .FirstOrDefaultAsync(l => l.Id == dto.LoanId)
            ?? throw new NotFoundException("Loan", dto.LoanId);

        if (loan.Status is not (LoanStatus.OpenForFunding or LoanStatus.PartiallyFunded))
            throw new InvalidLoanStateException("Bu loan hozir investitsiya uchun ochiq emas.");

        if (loan.OpenUntil < DateTimeOffset.UtcNow)
            throw new InvalidLoanStateException("Loan muddati tugagan.");

        if (dto.Amount < loan.MinContribution)
            throw new ValidationException("amount",
                $"Minimal investitsiya miqdori: {loan.MinContribution:N0} UZS");

        var remaining    = loan.Amount - loan.FundedAmount;
        var actualAmount = Math.Min(dto.Amount, remaining);
        if (actualAmount <= 0)
            throw new InvalidLoanStateException("Bu loan to'liq moliyalashtirilgan.");

        var lenderProfile = await _context.LenderProfiles
            .FirstOrDefaultAsync(lp => lp.UserId == lenderUserId)
            ?? throw new NotFoundException("LenderProfile topilmadi. Avval lender profilingizni yarating.");

        // ── Atomik operatsiya: transaction ────────────────────────────────
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Wallet dan yechish BIRINCHI (retry loop bo'lgani uchun oldin bajariladi)
            await _wallet.WithdrawAsync(
                lenderUserId, actualAmount,
                TransactionType.Investment,
                dto.LoanId,
                $"Loan #{dto.LoanId} ga investitsiya");

            // 2. Investment yozuvi
            var investment = new Investment
            {
                LoanId     = dto.LoanId,
                LenderId   = lenderProfile.Id,
                Amount     = actualAmount,
                InvestedAt = DateTimeOffset.UtcNow
            };
            _context.Investments.Add(investment);

            // 3. Loan FundedAmount yangilash (RowVersion orqali concurrency himoyasi)
            loan.FundedAmount += actualAmount;
            loan.Status = loan.FundedAmount >= loan.Amount
                ? LoanStatus.Funded
                : LoanStatus.PartiallyFunded;

            await _context.SaveChangesAsync(); // RowVersion conflict bu yerda tutiladi

            await tx.CommitAsync();

            // ── Audit va Notification (izolyatsiyalangan, tranzaksiyadan tashqarida) ──
            await _audit.LogAsync("Investment", investment.Id, "Created", lenderUserId,
                new { investment.Amount, dto.LoanId });

            if (loan.Borrower?.UserId != null)
                await _notifications.SendAsync(loan.Borrower.UserId,
                    "Yangi investitsiya",
                    $"'{loan.Title}' kreditingizga {actualAmount:N0} UZS investitsiya kiritildi.");

            if (loan.Status == LoanStatus.Funded && loan.Borrower?.UserId != null)
                await _notifications.SendAsync(loan.Borrower.UserId,
                    "Kreditingiz to'liq moliyalashtirildi!",
                    $"'{loan.Title}' kreditingiz {loan.Amount:N0} UZS to'liq moliyalashtirildi. Tasdiqlashingiz kerak.");

            return investment;
        }
        catch (DbUpdateConcurrencyException)
        {
            await tx.RollbackAsync();
            throw new ConflictException(
                "Bir vaqtda boshqa investor ham bu loanga investitsiya qildi. Qayta urinib ko'ring.");
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task WithdrawInvestmentAsync(Guid investmentId, Guid lenderUserId)
    {
        var investment = await _context.Investments
            .Include(i => i.Loan)
            .Include(i => i.Lender)
            .FirstOrDefaultAsync(i => i.Id == investmentId)
            ?? throw new NotFoundException("Investment", investmentId);

        if (investment.Lender?.UserId != lenderUserId)
            throw new UnauthorizedException("Bu investitsiya sizga tegishli emas.");

        if (investment.Loan?.Status is LoanStatus.Active or LoanStatus.Repayment
            or LoanStatus.Paid or LoanStatus.Overdue or LoanStatus.Default)
            throw new InvalidLoanStateException("Faol yoki tugagan kreditdan investitsiyani qaytarib bo'lmaydi.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Wallet ga qaytarish BIRINCHI
            await _wallet.DepositAsync(
                lenderUserId, investment.Amount,
                TransactionType.Refund,
                investment.LoanId,
                $"Loan #{investment.LoanId} dan investitsiya qaytarildi");

            // 2. Loan FundedAmount kamaytirish
            var loan = investment.Loan!;
            loan.FundedAmount -= investment.Amount;
            loan.Status = loan.FundedAmount > 0
                ? LoanStatus.PartiallyFunded
                : LoanStatus.OpenForFunding;

            _context.Investments.Remove(investment);
            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            await _audit.LogAsync("Investment", investmentId, "Withdrawn", lenderUserId,
                new { investment.Amount, investment.LoanId });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<InvestmentDto>> GetByLenderAsync(Guid lenderUserId)
    {
        var profile = await _context.LenderProfiles
            .AsNoTracking()
            .FirstOrDefaultAsync(lp => lp.UserId == lenderUserId);

        if (profile is null) return Enumerable.Empty<InvestmentDto>();

        return await _context.Investments
            .AsNoTracking()
            .Where(i => i.LenderId == profile.Id)
            .OrderByDescending(i => i.InvestedAt)
            .Select(i => new InvestmentDto
            {
                Id         = i.Id,
                LoanId     = i.LoanId,
                LenderId   = i.LenderId,
                Amount     = i.Amount,
                InvestedAt = i.InvestedAt
            })
            .ToListAsync();
    }

    public async Task<IEnumerable<InvestmentDto>> GetByLoanAsync(Guid loanId)
    {
        return await _context.Investments
            .AsNoTracking()
            .Where(i => i.LoanId == loanId)
            .OrderByDescending(i => i.InvestedAt)
            .Select(i => new InvestmentDto
            {
                Id         = i.Id,
                LoanId     = i.LoanId,
                LenderId   = i.LenderId,
                Amount     = i.Amount,
                InvestedAt = i.InvestedAt
            })
            .ToListAsync();
    }
}
