using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;

namespace P2PLoan.Core.Entities;
public class Wallet
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid UserId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Balance { get; set; } = 0m;
    [MaxLength(10)] public string Currency { get; set; } = "UZS";
    public DateTimeOffset UpdatedAt { get; set; } = DateTimeOffset.UtcNow;

    public User? User { get; set; }
    public ICollection<Transaction> Transactions { get; set; } = new List<Transaction>();
}
