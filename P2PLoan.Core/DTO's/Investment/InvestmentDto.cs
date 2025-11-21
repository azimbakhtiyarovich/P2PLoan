using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Investment;
public class InvestmentDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid LenderId { get; set; }
    public decimal Amount { get; set; }
    public DateTimeOffset InvestedAt { get; set; }
}
