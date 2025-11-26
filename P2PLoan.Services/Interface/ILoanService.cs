
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;

namespace P2PLoan.Services.Interface;
public interface ILoanService
{
    Task<Loan> CreateLoanAsync(Loan loan);
    Task<Loan?> GetLoanByIdAsync(Guid id);
    Task<IEnumerable<Loan>> GetLoansByBorrowerAsync(Guid borrowerId);
    Task<bool> UpdateLoanStatusAsync(Guid loanId, LoanStatus status);
}
