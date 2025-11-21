using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Contract;
public class InvestDto
{
    public Guid LoanId { get; set; }
    public decimal Amount { get; set; }
    public PaymentProvider PaymentProvider { get; set; } = PaymentProvider.Card;
}
