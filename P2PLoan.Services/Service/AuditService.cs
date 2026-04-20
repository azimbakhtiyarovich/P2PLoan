using System.Text.Json;
using P2PLoan.Core.Entities;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _context;

    public AuditService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task LogAsync(
        string entityType,
        Guid? entityId,
        string action,
        Guid? performedBy,
        object? details = null)
    {
        _context.AuditLogs.Add(new AuditLog
        {
            EntityType   = entityType,
            EntityId     = entityId,
            Action       = action,
            PerformedBy  = performedBy,
            DetailsJson  = details is not null
                ? JsonSerializer.Serialize(details)
                : null
        });

        await _context.SaveChangesAsync();
    }
}
