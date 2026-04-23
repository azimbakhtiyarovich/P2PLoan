using P2PLoan.Core.DTO.Investment;
using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;

public interface IInvestmentService
{
    /// <summary>User investitsiya qiladi. Wallet balansidan yechadi, loan FundedAmount ni oshiradi.</summary>
    Task<Investment> InvestAsync(Guid userId, InvestDto dto);

    /// <summary>User investitsiyasini qaytaradi (loan Active bo'lgunga qadar).</summary>
    Task WithdrawInvestmentAsync(Guid investmentId, Guid userId);

    Task<IEnumerable<InvestmentDto>> GetByUserAsync(Guid userId);

    Task<IEnumerable<InvestmentDto>> GetByLoanAsync(Guid loanId);
}
