using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;

namespace P2PLoan.Services.Interface;

public interface IPaymentService
{
    /// <summary>Yangi to'lov yaratadi (deposit uchun).</summary>
    Task<Payment> CreateDepositAsync(Guid userId, decimal amount, PaymentProvider provider);

    /// <summary>Provider callbackini qayta ishlaydi va to'lov statusini yangilaydi.</summary>
    Task ProcessCallbackAsync(PaymentCallbackDto callback);

    /// <summary>Repayment ni to'laydi (wallet → repayment).</summary>
    Task PayRepaymentAsync(Guid repaymentId, Guid borrowerUserId);

    Task<IEnumerable<Payment>> GetByUserAsync(Guid userId);
}
