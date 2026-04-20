using P2PLoan.Core.DTO.Investment;
using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;

public interface IInvestmentService
{
    /// <summary>Lender investitsiya qiladi. Wallet balansidan yechadi, loan FundedAmount ni oshiradi.</summary>
    Task<Investment> InvestAsync(Guid lenderUserId, InvestDto dto);

    /// <summary>Lender investitsiyasini qaytaradi (loan Active bo'lgunga qadar).</summary>
    Task WithdrawInvestmentAsync(Guid investmentId, Guid lenderUserId);

    Task<IEnumerable<InvestmentDto>> GetByLenderAsync(Guid lenderUserId);

    Task<IEnumerable<InvestmentDto>> GetByLoanAsync(Guid loanId);
}
