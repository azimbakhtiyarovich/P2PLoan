using P2PLoan.Core.Enum_s;

namespace P2PLoan.Core.Entities;
public class Loan
{
    public Guid Id { get; set; }
    public Guid BorrowerId { get; set; }
    public decimal Amount { get; set; }
    public double InterestRate { get; set; }
    public int DurationMonths { get; set; }
    public LoanStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }

    public User Borrower { get; set; }
    public Contract Contract { get; set; }
}

