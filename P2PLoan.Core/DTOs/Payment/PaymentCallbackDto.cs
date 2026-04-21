using P2PLoan.Core.Enum;

namespace P2PLoan.Core.DTO.Payment;

public class PaymentCallbackDto
{
    public PaymentProvider Provider { get; set; }
    public string ExternalId { get; set; } = null!;
    public PaymentStatus Status { get; set; }
    public decimal Amount { get; set; }
    public string? MetaJson { get; set; }
}
