using P2PLoan.Core.DTO.Loan;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Enum;

namespace P2PLoan.Services.Interface;

public interface ILoanService
{
    /// <summary>Kredit tekshiruv bilan yangi loan yaratadi va Repayment schedule generatsiya qiladi.</summary>
    Task<Loan> CreateLoanAsync(CreateLoanDto dto, Guid borrowerProfileId);

    Task<Loan?> GetLoanByIdAsync(Guid id);

    Task<IEnumerable<Loan>> GetLoansByBorrowerAsync(Guid borrowerProfileId);

    /// <summary>Barcha ochiq loanlarni qaytaradi (lender uchun).</summary>
    Task<IEnumerable<Loan>> GetOpenLoansAsync(int page = 1, int pageSize = 20);

    /// <summary>Loan statusini state-machine qoidalariga rioya qilib yangilaydi.</summary>
    Task<bool> UpdateLoanStatusAsync(Guid loanId, LoanStatus newStatus, Guid? performedBy = null);

    /// <summary>Borrower loanini qabul qiladi (AcceptedByBorrower → Active).</summary>
    Task AcceptLoanAsync(Guid loanId, Guid borrowerUserId);

    /// <summary>Loan uchun repayment schedule ni qaytaradi.</summary>
    Task<IEnumerable<Repayment>> GetRepaymentScheduleAsync(Guid loanId);
}
