using P2PLoan.Core.Enum;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace P2PLoan.Core.Entities;
public class Loan
{
    [Key] public Guid Id { get; set; } = Guid.NewGuid();
    public Guid BorrowerId { get; set; }
    [MaxLength(255)] public string? Title { get; set; }
    public string? Description { get; set; }

    [Required, Column(TypeName = "decimal(18,2)")] public decimal Amount { get; set; }
    [Column(TypeName = "decimal(18,2)")] public decimal FundedAmount { get; set; } = 0m;
    [Column(TypeName = "decimal(18,2)")] public decimal MinContribution { get; set; } = 1m;

    public int DurationDays { get; set; }
    [Column(TypeName = "decimal(5,2)")] public decimal InterestRate { get; set; } // percent
    public RepaymentFrequency RepaymentFrequency { get; set; } = RepaymentFrequency.Monthly;
    public LoanStatus Status { get; set; } = LoanStatus.Created;

    public DateTimeOffset CreatedAt { get; set; } = DateTimeOffset.UtcNow;
    public DateTimeOffset? OpenUntil { get; set; }
    public DateTimeOffset? StartDate { get; set; } // when Active
    public bool AcceptedByBorrower { get; set; } = false;

    // nav
    public BorrowerProfile? Borrower { get; set; }
    public ICollection<LoanOffer> Offers { get; set; } = new List<LoanOffer>();
    public ICollection<Investment> Investments { get; set; } = new List<Investment>();
    public ICollection<Repayment> Repayments { get; set; } = new List<Repayment>();
}

