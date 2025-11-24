using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO.Payment;
public class CreatePaymentDto
{
    public Guid? UserId { get; set; }
    public decimal Amount { get; set; }
    public PaymentProvider Provider { get; set; } = PaymentProvider.Card;
    public string? ReturnUrl { get; set; } // for redirect flows
}
