using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO.Payment;
public class PaymentCallbackDto
{
    public string Provider { get; set; } = null!;
    public string ExternalId { get; set; } = null!;
    public string Status { get; set; } = null!;
    public decimal Amount { get; set; }
    public string? MetaJson { get; set; }
}
