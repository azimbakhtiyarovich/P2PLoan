using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;

namespace P2PLoan.Services.Interface;

public interface IWalletService
{
    Task<Wallet> GetOrCreateWalletAsync(Guid userId);

    /// <summary>Hisobga pul kirim qiladi va Transaction yozadi.</summary>
    Task<Wallet> DepositAsync(Guid userId, decimal amount, TransactionType txType = TransactionType.Deposit, Guid? referenceId = null, string? meta = null);

    /// <summary>
    /// Hisobdan pul chiqaradi. Mablag' yetarli bo'lmasa InsufficientFundsException.
    /// Optimistic concurrency bilan himoyalangan.
    /// </summary>
    Task<Wallet> WithdrawAsync(Guid userId, decimal amount, TransactionType txType, Guid? referenceId = null, string? meta = null);

    Task<WalletDto> GetBalanceAsync(Guid userId);
    Task<IEnumerable<TransactionDto>> GetTransactionsAsync(Guid userId, int page = 1, int pageSize = 20);
}
