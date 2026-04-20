namespace P2PLoan.Core.Exceptions;

/// <summary>
/// Barcha domain exception larning asosi. HTTP status code bilan keladi.
/// </summary>
public abstract class AppException : Exception
{
    public int StatusCode { get; }

    protected AppException(string message, int statusCode) : base(message)
    {
        StatusCode = statusCode;
    }
}
