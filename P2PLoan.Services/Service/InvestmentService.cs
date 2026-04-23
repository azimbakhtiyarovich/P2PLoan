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

    public async Task<Investment> InvestAsync(Guid userId, InvestDto dto)
    {
        // ── Validatsiya ──────────────────────────────────────────────────
        var loan = await _context.Loans
            .FirstOrDefaultAsync(l => l.Id == dto.LoanId)
            ?? throw new NotFoundException("Loan", dto.LoanId);

        // O'z loaniga investitsiya qilish taqiqi
        if (loan.UserId == userId)
            throw new ValidationException("loanId", "O'z kreditingizga investitsiya qila olmaysiz.");

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

        // ── Atomik operatsiya: transaction ────────────────────────────────
        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Wallet dan yechish
            await _wallet.WithdrawAsync(
                userId, actualAmount,
                TransactionType.Investment,
                dto.LoanId,
                $"Loan #{dto.LoanId} ga investitsiya");

            // 2. Investment yozuvi
            var investment = new Investment
            {
                LoanId     = dto.LoanId,
                UserId     = userId,
                Amount     = actualAmount,
                InvestedAt = DateTimeOffset.UtcNow
            };
            _context.Investments.Add(investment);

            // 3. Loan FundedAmount yangilash
            loan.FundedAmount += actualAmount;
            loan.Status = loan.FundedAmount >= loan.Amount
                ? LoanStatus.Funded
                : LoanStatus.PartiallyFunded;

            await _context.SaveChangesAsync();

            await tx.CommitAsync();

            // ── Audit va Notification ──
            await _audit.LogAsync("Investment", investment.Id, "Created", userId,
                new { investment.Amount, dto.LoanId });

            await _notifications.SendAsync(loan.UserId,
                "Yangi investitsiya",
                $"'{loan.Title}' kreditingizga {actualAmount:N0} UZS investitsiya kiritildi.");

            if (loan.Status == LoanStatus.Funded)
                await _notifications.SendAsync(loan.UserId,
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

    public async Task WithdrawInvestmentAsync(Guid investmentId, Guid userId)
    {
        var investment = await _context.Investments
            .Include(i => i.Loan)
            .FirstOrDefaultAsync(i => i.Id == investmentId)
            ?? throw new NotFoundException("Investment", investmentId);

        if (investment.UserId != userId)
            throw new UnauthorizedException("Bu investitsiya sizga tegishli emas.");

        if (investment.Loan?.Status is LoanStatus.Active or LoanStatus.Repayment
            or LoanStatus.Paid or LoanStatus.Overdue or LoanStatus.Default)
            throw new InvalidLoanStateException("Faol yoki tugagan kreditdan investitsiyani qaytarib bo'lmaydi.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Wallet ga qaytarish
            await _wallet.DepositAsync(
                userId, investment.Amount,
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

            await _audit.LogAsync("Investment", investmentId, "Withdrawn", userId,
                new { investment.Amount, investment.LoanId });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task<IEnumerable<InvestmentDto>> GetByUserAsync(Guid userId)
    {
        return await _context.Investments
            .AsNoTracking()
            .Where(i => i.UserId == userId)
            .OrderByDescending(i => i.InvestedAt)
            .Select(i => new InvestmentDto
            {
                Id         = i.Id,
                LoanId     = i.LoanId,
                UserId     = i.UserId,
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
                UserId     = i.UserId,
                Amount     = i.Amount,
                InvestedAt = i.InvestedAt
            })
            .ToListAsync();
    }
}
