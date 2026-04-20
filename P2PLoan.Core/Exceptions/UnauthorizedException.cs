namespace P2PLoan.Core.Exceptions;

public sealed class UnauthorizedException : AppException
{
    public UnauthorizedException(string message = "Kirish taqiqlangan.")
        : base(message, 401) { }
}
