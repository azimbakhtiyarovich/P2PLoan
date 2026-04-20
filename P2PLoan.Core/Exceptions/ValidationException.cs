namespace P2PLoan.Core.Exceptions;

public sealed class ValidationException : AppException
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(string field, string error)
        : base($"Validatsiya xatosi: {field} - {error}", 400)
    {
        Errors = new Dictionary<string, string[]> { { field, new[] { error } } };
    }

    public ValidationException(IDictionary<string, string[]> errors)
        : base("Validatsiya xatosi", 400)
    {
        Errors = new Dictionary<string, string[]>(errors);
    }
}
