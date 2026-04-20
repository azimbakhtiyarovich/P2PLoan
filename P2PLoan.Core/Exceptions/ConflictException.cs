namespace P2PLoan.Core.Exceptions;

public sealed class ConflictException : AppException
{
    public ConflictException(string message)
        : base(message, 409) { }
}
