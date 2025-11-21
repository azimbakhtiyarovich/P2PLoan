using P2PLoan.Core.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace P2PLoan.Core.DTO_s.Loan;
public class LoanOfferDto
{
    public Guid Id { get; set; }
    public Guid LoanId { get; set; }
    public Guid? LenderId { get; set; }
    public decimal OfferedAmount { get; set; }
    public decimal? OfferedRate { get; set; }
    public OfferStatus Status { get; set; }
}
