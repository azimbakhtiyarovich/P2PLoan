using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO.Payment;
public class TransactionDto 
{
    public Guid Id { get; set; }
    public TransactionType Type { get; set; }
    public decimal Amount { get; set; }
    public decimal BalanceAfter { get; set; } 
    public DateTimeOffset CreatedAt { get; set; }
}
