namespace P2PLoan.Services.Interface;

public interface IAuditService
{
    Task LogAsync(
        string entityType,
        Guid? entityId,
        string action,
        Guid? performedBy,
        object? details = null);
}
