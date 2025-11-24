using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Repayment
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanId { get; set; }
    public DateTime DueDate { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PrincipalAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal InterestAmount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal PaidAmount { get; set; } = 0m;

    public PaymentStatus Status { get; set; } = PaymentStatus.Created;
    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? PaidAt { get; set; }

    public Loan? Loan { get; set; }
}
