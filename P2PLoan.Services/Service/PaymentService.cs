using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;
    private readonly IWalletService       _wallet;
    private readonly INotificationService _notifications;
    private readonly IAuditService        _audit;

    public PaymentService(
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

    public async Task<Payment> CreateDepositAsync(
        Guid userId, decimal amount, PaymentProvider provider)
    {
        if (amount <= 0)
            throw new ValidationException("amount", "Depozit summasi musbat bo'lishi kerak.");

        var payment = new Payment
        {
            UserId   = userId,
            Amount   = amount,
            Provider = provider,
            Status   = PaymentStatus.Processing
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        await _audit.LogAsync("Payment", payment.Id, "DepositCreated", userId,
            new { amount, provider });

        return payment;
    }

    public async Task ProcessCallbackAsync(PaymentCallbackDto callback)
    {
        // ExternalId orqali payment ni topish
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.ExternalId == callback.ExternalId
                                   && p.Provider == callback.Provider)
            ?? throw new NotFoundException($"Payment ExternalId={callback.ExternalId} topilmadi");

        if (payment.Status == PaymentStatus.Success)
            return; // Idempotent: ikki marta qayta ishlash xavfsiz

        var prevStatus = payment.Status;
        payment.Status     = callback.Status;
        payment.ExternalId = callback.ExternalId;
        payment.UpdatedAt  = DateTimeOffset.UtcNow;

        // Muvaffaqiyatli to'lov → wallet ga depozit
        if (callback.Status == PaymentStatus.Success && payment.UserId.HasValue)
        {
            await _wallet.DepositAsync(
                payment.UserId.Value,
                payment.Amount,
                payment.Id,
                $"Depozit via {payment.Provider}");

            await _notifications.SendAsync(payment.UserId.Value,
                "Hisobingizga pul kelib tushdi",
                $"{payment.Amount:N0} UZS hisobingizga muvaffaqiyatli kiritildi ({payment.Provider}).");
        }

        await _context.SaveChangesAsync();

        await _audit.LogAsync("Payment", payment.Id, "CallbackProcessed", payment.UserId,
            new { From = prevStatus, To = callback.Status });
    }

    public async Task PayRepaymentAsync(Guid repaymentId, Guid borrowerUserId)
    {
        var repayment = await _context.Repayments
            .Include(r => r.Loan)
                .ThenInclude(l => l!.Borrower)
            .FirstOrDefaultAsync(r => r.Id == repaymentId)
            ?? throw new NotFoundException("Repayment", repaymentId);

        if (repayment.Loan?.Borrower?.UserId != borrowerUserId)
            throw new UnauthorizedException("Bu to'lov sizga tegishli emas.");

        if (repayment.Status == PaymentStatus.Success)
            throw new ConflictException("Bu to'lov allaqachon amalga oshirilgan.");

        var due = repayment.Amount - repayment.PaidAmount;
        if (due <= 0) throw new ConflictException("Bu to'lov allaqachon to'liq to'langan.");

        // Wallet dan yechish
        await _wallet.WithdrawAsync(
            borrowerUserId, due,
            TransactionType.RepaymentReceived,
            repaymentId,
            $"Repayment #{repaymentId}");

        repayment.PaidAmount += due;
        repayment.PaidAt = DateTimeOffset.UtcNow;
        repayment.Status = PaymentStatus.Success;

        // Lenderlarga foydani taqsimlash
        await DistributeProfitToLendersAsync(repayment);

        // Barcha repayment lar to'langan bo'lsa loan Paid holatiga o'tkazish
        await CheckAndFinalizeLoanAsync(repayment.LoanId);

        await _context.SaveChangesAsync();

        await _notifications.SendAsync(borrowerUserId, "To'lov amalga oshirildi",
            $"{due:N0} UZS muvaffaqiyatli to'landi.");

        await _audit.LogAsync("Repayment", repaymentId, "Paid", borrowerUserId,
            new { Amount = due });
    }

    public async Task<IEnumerable<Payment>> GetByUserAsync(Guid userId)
    {
        return await _context.Payments
            .AsNoTracking()
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();
    }

    // ── Private Helpers ───────────────────────────────────────────────────────

    /// <summary>
    /// Har bir repayment to'langanda, investorlar ulushiga qarab foydani
    /// wallet ga kiritadi (ProfitCredit transaction).
    /// </summary>
    private async Task DistributeProfitToLendersAsync(Repayment repayment)
    {
        var loan = repayment.Loan!;
        var investments = await _context.Investments
            .Include(i => i.Lender)
            .Where(i => i.LoanId == loan.Id)
            .ToListAsync();

        if (!investments.Any() || loan.FundedAmount <= 0) return;

        foreach (var inv in investments)
        {
            if (inv.Lender?.UserId is null) continue;

            // Investor ulushi = uning miqdori / umumiy moliyalashtirilgan summa
            var share = inv.Amount / loan.FundedAmount;

            var principalShare = Math.Round(repayment.PrincipalAmount * share, 2);
            var interestShare  = Math.Round(repayment.InterestAmount * share, 2);
            var total          = principalShare + interestShare;

            await _wallet.DepositAsync(
                inv.Lender.UserId,
                total,
                repayment.Id,
                $"Loan #{loan.Id} dan to'lov: asosiy={principalShare:N2}, foiz={interestShare:N2}");

            await _notifications.SendAsync(inv.Lender.UserId, "Daromad keldi",
                $"{total:N0} UZS hisobingizga kiritildi (kredit to'lovi).");
        }
    }

    private async Task CheckAndFinalizeLoanAsync(Guid loanId)
    {
        var allPaid = await _context.Repayments
            .Where(r => r.LoanId == loanId)
            .AllAsync(r => r.Status == PaymentStatus.Success);

        if (!allPaid) return;

        var loan = await _context.Loans.FindAsync(loanId);
        if (loan is null) return;

        loan.Status = LoanStatus.Paid;
    }
}
