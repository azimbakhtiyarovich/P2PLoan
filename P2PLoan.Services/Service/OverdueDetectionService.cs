using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using P2PLoan.Core.Enum;
using P2PLoan.DataAccess;

namespace P2PLoan.Services.Service;

/// <summary>
/// Har soatda ishga tushadigan background servis.
/// Muddati o'tgan loanlarni Overdue holatiga o'tkazadi.
/// FIX #15: LoanStatus.Overdue hech qachon ishlatilmasdi — endi avtomatik qo'llaniladi.
/// </summary>
public class OverdueDetectionService : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<OverdueDetectionService> _logger;

    private static readonly TimeSpan CheckInterval = TimeSpan.FromHours(1);

    public OverdueDetectionService(
        IServiceScopeFactory scopeFactory,
        ILogger<OverdueDetectionService> logger)
    {
        _scopeFactory = scopeFactory;
        _logger       = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("OverdueDetectionService ishga tushdi.");

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await DetectOverdueLoansAsync(stoppingToken);
            }
            catch (Exception ex) when (ex is not OperationCanceledException)
            {
                _logger.LogError(ex, "Overdue tekshiruvida xato.");
            }

            await Task.Delay(CheckInterval, stoppingToken);
        }
    }

    private async Task DetectOverdueLoansAsync(CancellationToken ct)
    {
        using var scope   = _scopeFactory.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var now     = DateTimeOffset.UtcNow;

        // Faol loanlar ichidan muddati o'tgan repayment'i bor loanlani topish
        var overdueLoans = await context.Loans
            .Where(l => l.Status == LoanStatus.Active || l.Status == LoanStatus.Repayment)
            .Where(l => l.Repayments.Any(r =>
                r.Status != PaymentStatus.Success &&
                r.DueDate < now))
            .ToListAsync(ct);

        if (!overdueLoans.Any()) return;

        foreach (var loan in overdueLoans)
            loan.Status = LoanStatus.Overdue;

        var updated = await context.SaveChangesAsync(ct);
        if (updated > 0)
            _logger.LogWarning("{Count} ta loan Overdue holatiga o'tkazildi.", updated);
    }
}
