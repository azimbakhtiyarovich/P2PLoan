using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Loan;
public class LoanSummaryDto
{
    public Guid Id { get; set; }
    public string? Title { get; set; }
    public decimal Amount { get; set; }
    public decimal FundedAmount { get; set; }
    public int DurationDays { get; set; }
    public decimal InterestRate { get; set; }
    public LoanStatus Status { get; set; }
}
