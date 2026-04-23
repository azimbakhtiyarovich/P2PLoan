using P2PLoan.Core.Enum;

namespace P2PLoan.Core.DTO.Payment;
public class RepaymentDto
{
    public Guid Id { get; set; }
    public DateTimeOffset DueDate { get; set; }
    public decimal Amount { get; set; }
    public decimal PrincipalAmount { get; set; }
    public decimal InterestAmount { get; set; }
    public decimal PaidAmount { get; set; }
    public PaymentStatus Status { get; set; }
}

