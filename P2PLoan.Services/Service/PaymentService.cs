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
        var payment = await _context.Payments
            .FirstOrDefaultAsync(p => p.ExternalId == callback.ExternalId
                                   && p.Provider == callback.Provider)
            ?? throw new NotFoundException($"Payment ExternalId={callback.ExternalId} topilmadi");

        if (payment.Status == PaymentStatus.Success) return;

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            var prevStatus = payment.Status;
            payment.Status     = callback.Status;
            payment.ExternalId = callback.ExternalId;
            payment.UpdatedAt  = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync();

            if (callback.Status == PaymentStatus.Success && payment.UserId.HasValue)
            {
                await _wallet.DepositAsync(
                    payment.UserId.Value,
                    payment.Amount,
                    TransactionType.Deposit,
                    payment.Id,
                    $"Depozit via {payment.Provider}");
            }

            await tx.CommitAsync();

            await _audit.LogAsync("Payment", payment.Id, "CallbackProcessed", payment.UserId,
                new { From = prevStatus, To = callback.Status });

            if (callback.Status == PaymentStatus.Success && payment.UserId.HasValue)
            {
                await _notifications.SendAsync(payment.UserId.Value,
                    "Hisobingizga pul kelib tushdi",
                    $"{payment.Amount:N0} UZS hisobingizga muvaffaqiyatli kiritildi ({payment.Provider}).");
            }
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
    }

    public async Task PayRepaymentAsync(Guid repaymentId, Guid borrowerUserId)
    {
        var repayment = await _context.Repayments
            .Include(r => r.Loan)
            .FirstOrDefaultAsync(r => r.Id == repaymentId)
            ?? throw new NotFoundException("Repayment", repaymentId);

        if (repayment.Loan?.UserId != borrowerUserId)
            throw new UnauthorizedException("Bu to'lov sizga tegishli emas.");

        if (repayment.Status == PaymentStatus.Success)
            throw new ConflictException("Bu to'lov allaqachon amalga oshirilgan.");

        var due = repayment.Amount - repayment.PaidAmount;
        if (due <= 0) throw new ConflictException("Bu to'lov allaqachon to'liq to'langan.");

        await using var tx = await _context.Database.BeginTransactionAsync();
        try
        {
            // 1. Borrower walletdan yechish
            await _wallet.WithdrawAsync(
                borrowerUserId, due,
                TransactionType.RepaymentReceived,
                repaymentId,
                $"Repayment #{repaymentId}");

            // 2. Repayment yangilash
            repayment.PaidAmount += due;
            repayment.PaidAt     = DateTimeOffset.UtcNow;
            repayment.Status     = PaymentStatus.Success;
            await _context.SaveChangesAsync();

            // 3. Investorlarga foyda taqsimlash
            await DistributeProfitToInvestorsAsync(repayment);

            // 4. Loan tugadimi tekshirish
            await CheckAndFinalizeLoanAsync(repayment.LoanId);

            await tx.CommitAsync();

            await _notifications.SendAsync(borrowerUserId, "To'lov amalga oshirildi",
                $"{due:N0} UZS muvaffaqiyatli to'landi.");

            await _audit.LogAsync("Repayment", repaymentId, "Paid", borrowerUserId,
                new { Amount = due });
        }
        catch
        {
            await tx.RollbackAsync();
            throw;
        }
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

    private async Task DistributeProfitToInvestorsAsync(Repayment repayment)
    {
        var loan = repayment.Loan!;
        var investments = await _context.Investments
            .Where(i => i.LoanId == loan.Id)
            .ToListAsync();

        if (!investments.Any() || loan.FundedAmount <= 0) return;

        foreach (var inv in investments)
        {
            var share          = inv.Amount / loan.FundedAmount;
            var principalShare = Math.Round(repayment.PrincipalAmount * share, 2);
            var interestShare  = Math.Round(repayment.InterestAmount * share, 2);
            var total          = principalShare + interestShare;

            await _wallet.DepositAsync(
                inv.UserId,
                total,
                TransactionType.ProfitCredit,
                repayment.Id,
                $"Loan #{loan.Id}: asosiy={principalShare:N2}, foiz={interestShare:N2}");

            await _notifications.SendAsync(inv.UserId, "Daromad keldi",
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
        await _context.SaveChangesAsync();
    }
}
