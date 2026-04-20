using Microsoft.EntityFrameworkCore;
using P2PLoan.Core.Entities;
using P2PLoan.Core.Exceptions;
using P2PLoan.DataAccess;
using P2PLoan.Services.Interface;

namespace P2PLoan.Services.Service;

public class NotificationService : INotificationService
{
    private readonly ApplicationDbContext _context;

    public NotificationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task SendAsync(Guid userId, string title, string message)
    {
        _context.Notifications.Add(new Notification
        {
            UserId  = userId,
            Title   = title,
            Message = message,
            Read    = false
        });
        await _context.SaveChangesAsync();
    }

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId)
            ?? throw new NotFoundException("Notification", notificationId);

        notification.Read = true;
        await _context.SaveChangesAsync();
    }

    public async Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId && !n.Read)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<IEnumerable<Notification>> GetAllAsync(
        Guid userId, int page = 1, int pageSize = 20)
    {
        return await _context.Notifications
            .AsNoTracking()
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();
    }
}
