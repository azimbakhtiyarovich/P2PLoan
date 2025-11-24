using P2PLoan.Core.DTO.Payment;

namespace P2PLoan.Core.DTO.Loan;
public class LoanDetailDto:LoanSummaryDto
{
    public string? Description { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public IEnumerable<LoanOfferDto>? Offers { get; set; }
    public IEnumerable<RepaymentDto>? Repayments { get; set; }
}

