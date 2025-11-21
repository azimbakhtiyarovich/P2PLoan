using P2PLoan.Core.Enum_s;

namespace P2PLoan.Core.DTO_s.Payment;
public class PaymentDto
{
    public Guid ContractId { get; set; }
    public DateTime DueDate { get; set; }
    public decimal Amount { get; set; }
    public PaymentStatus Status { get; set; }
}

