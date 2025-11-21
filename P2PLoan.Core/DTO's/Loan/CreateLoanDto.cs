using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Loan;
public class CreateLoanDto
{
    public decimal Amount { get; set; }
    public decimal? MinContribution { get; set; } = 1m;
    public int DurationDays { get; set; }
    public decimal InterestRate { get; set; }
    public RepaymentFrequency Frequency { get; set; } = RepaymentFrequency.Monthly;
    public string? Title { get; set; }
    public string? Description { get; set; }
    public DateTimeOffset? OpenUntil { get; set; }
}
