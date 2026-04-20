namespace P2PLoan.Core.Exceptions;

public sealed class NotFoundException : AppException
{
    public NotFoundException(string entity, object id)
        : base($"{entity} topilmadi: {id}", 404) { }

    public NotFoundException(string message)
        : base(message, 404) { }
}
