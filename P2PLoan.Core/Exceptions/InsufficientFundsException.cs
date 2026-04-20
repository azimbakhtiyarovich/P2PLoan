namespace P2PLoan.Core.Exceptions;

public sealed class InsufficientFundsException : AppException
{
    public InsufficientFundsException(decimal required, decimal available)
        : base($"Hisobda mablag' yetarli emas. Kerak: {required:N2}, Mavjud: {available:N2}", 422) { }
}
