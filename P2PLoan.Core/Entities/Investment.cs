using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2PLoan.Core.Entities;

public class Investment
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid LoanId { get; set; }
    public Guid UserId { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    public DateTimeOffset InvestedAt { get; set; } = DateTimeOffset.UtcNow;

    public Guid? PaymentId { get; set; }
    public Loan? Loan { get; set; }
    public User? User { get; set; }
}
