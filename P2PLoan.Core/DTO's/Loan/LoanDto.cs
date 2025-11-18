using P2PLoan.Core.Enum_s;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Loan;
public class LoanDto
{
    public Guid Id { get; set; }
    public decimal Amount { get; set; }
    public double InterestRate { get; set; }
    public int DurationMonths { get; set; }
    public LoanStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
}

