using P2PLoan.Core.DTO_s.Payment;
using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Loan;
public class LoanDetailDto:LoanSummaryDto
{
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IEnumerable<LoanOfferDto>? Offers { get; set; }
    public IEnumerable<RepaymentDto>? Repayments { get; set; }
}

