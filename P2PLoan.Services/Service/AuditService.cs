using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using P2PLoan.Core.Entities;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

/// <summary>
/// Audit loglarni caller ning DbContext'idan IZOLYATSIYALANGAN holda saqlaydi.
/// IServiceScopeFactory orqali yangi scope yaratib, alohida DbContext ishlatadi.
/// Natijada:
///   - Caller context dagi pending o'zgarishlarga tegmaydi (BUG #4 fix)
///   - Asosiy transaction muvaffaqiyatsiz bo'lsa ham audit log saqlanadi
/// </summary>
public class AuditService : IAuditService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public AuditService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public async Task LogAsync(
        string entityType,
        Guid? entityId,
        string action,
        Guid? performedBy,
        object? details = null)
    {
        try
        {
            using var scope   = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

            context.AuditLogs.Add(new AuditLog
            {
                EntityType  = entityType,
                EntityId    = entityId,
                Action      = action,
                PerformedBy = performedBy,
                DetailsJson = details is not null
                    ? JsonSerializer.Serialize(details)
                    : null
            });

            await context.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            // Audit log yozib bo'lmasa asosiy operatsiyani to'xtatmaymiz
            // Production da bu yerda Serilog/structured logging bo'lishi kerak
            System.Diagnostics.Debug.WriteLine($"[AuditService] Log yozishda xato: {ex.Message}");
        }
    }
}
