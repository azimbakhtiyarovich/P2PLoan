using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.Entities;
public class Investment
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanId { get; set; }
    public Guid LenderId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public DateTimeOffset InvestedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? PaymentId { get; set; } // link to payment record
    public Loan? Loan { get; set; }
    public LenderProfile? Lender { get; set; }
}
