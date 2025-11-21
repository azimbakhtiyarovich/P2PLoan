using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Transaction
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid WalletId { get; set; }
    public TransactionType Type { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal BalanceAfter { get; set; }
    public Guid? ReferenceId { get; set; } // payment/investment/repayment id
    public string? MetaJson { get; set; }
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;

    public Wallet? Wallet { get; set; }
}
