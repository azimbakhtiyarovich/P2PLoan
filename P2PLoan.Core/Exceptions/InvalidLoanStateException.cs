using P2PLoan.Core.Enum;

namespace P2PLoan.Core.Exceptions;

public sealed class InvalidLoanStateException : AppException
{
    public InvalidLoanStateException(LoanStatus current, LoanStatus expected)
        : base($"Loan holati noto'g'ri. Hozirgi: {current}, Talab qilingan: {expected}", 422) { }

    public InvalidLoanStateException(string message)
        : base(message, 422) { }
}
