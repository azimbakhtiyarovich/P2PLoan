using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2PLoan.Core.Entities;

public class Wallet
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Balance { get; set; } = 0m;
    [MaxLength(10)] public string Currency { get; set; } = "UZS";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    /// <summary>
    /// Optimistic concurrency - parallel tranzaksiyalarda double-spending oldini oladi.
    /// EF Core avtomatik tekshiradi: eski RowVersion != yangi → ConcurrencyException.
    /// </summary>
    [Timestamp] public byte[] RowVersion { get; set; } = null!;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
