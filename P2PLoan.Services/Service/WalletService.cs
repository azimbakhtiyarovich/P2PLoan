using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.DTO.Payment;
using P2PLoan.Core.DTO.Profile;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class WalletService : IWalletService
{
    private readonly ApplicationDbContext _context;

    public WalletService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<Wallet> GetOrCreateWalletAsync(Guid userId)
    {
        var wallet = await _context.Wallets.FirstOrDefaultAsync(w => w.UserId == userId);
        if (wallet is not null) return wallet;

        wallet = new Wallet { UserId = userId };
        _context.Wallets.Add(wallet);
        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> DepositAsync(
        Guid userId, decimal amount, Guid? referenceId = null, string? meta = null)
    {
        if (amount <= 0)
            throw new ValidationException("amount", "Depozit summasi musbat bo'lishi kerak.");

        var wallet = await GetOrCreateWalletAsync(userId);

        wallet.Balance += amount;
        wallet.UpdatedAt = DateTimeOffset.UtcNow;

        _context.Transactions.Add(new Transaction
        {
            WalletId     = wallet.Id,
            Type         = TransactionType.Deposit,
            Amount       = amount,
            BalanceAfter = wallet.Balance,
            ReferenceId  = referenceId,
            MetaJson     = meta
        });

        await _context.SaveChangesAsync();
        return wallet;
    }

    public async Task<Wallet> WithdrawAsync(
        Guid userId, decimal amount, TransactionType txType,
        Guid? referenceId = null, string? meta = null)
    {
        if (amount <= 0)
            throw new ValidationException("amount", "Chiqarish summasi musbat bo'lishi kerak.");

        // Retry loop: optimistic concurrency uchun
        for (var attempt = 0; attempt < 3; attempt++)
        {
            var wallet = await _context.Wallets
                .FirstOrDefaultAsync(w => w.UserId == userId)
                ?? throw new NotFoundException("Wallet", userId);

            if (wallet.Balance < amount)
                throw new InsufficientFundsException(amount, wallet.Balance);

            wallet.Balance -= amount;
            wallet.UpdatedAt = DateTimeOffset.UtcNow;

            _context.Transactions.Add(new Transaction
            {
                WalletId     = wallet.Id,
                Type         = txType,
                Amount       = -amount,
                BalanceAfter = wallet.Balance,
                ReferenceId  = referenceId,
                MetaJson     = meta
            });

            try
            {
                await _context.SaveChangesAsync();
                return wallet;
            }
            catch (DbUpdateConcurrencyException)
            {
                // Boshqa parallel request yangiladi — retry
                _context.ChangeTracker.Clear();
                if (attempt == 2) throw;
            }
        }

        throw new InvalidOperationException("Concurrency xatosi: 3 ta urinishdan keyin ham muvaffaqiyatsiz.");
    }

    public async Task<WalletDto> GetBalanceAsync(Guid userId)
    {
        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId)
            ?? await GetOrCreateWalletAsync(userId);

        return new WalletDto { UserId = userId, Balance = wallet.Balance };
    }

    public async Task<IEnumerable<TransactionDto>> GetTransactionsAsync(
        Guid userId, int page = 1, int pageSize = 20)
    {
        var wallet = await _context.Wallets
            .AsNoTracking()
            .FirstOrDefaultAsync(w => w.UserId == userId);

        if (wallet is null) return Enumerable.Empty<TransactionDto>();

        return await _context.Transactions
            .AsNoTracking()
            .Where(t => t.WalletId == wallet.Id)
            .OrderByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => new TransactionDto
            {
                Id           = t.Id,
                Type         = t.Type,
                Amount       = t.Amount,
                BalanceAfter = t.BalanceAfter,
                CreatedAt    = t.CreatedAt
            })
            .ToListAsync();
    }
}
