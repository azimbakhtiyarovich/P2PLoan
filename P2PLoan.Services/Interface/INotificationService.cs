using P2PLoan.Core.Entities;

namespace P2PLoan.Services.Interface;

public interface INotificationService
{
    Task SendAsync(Guid userId, string title, string message);

    Task MarkAsReadAsync(Guid notificationId, Guid userId);

    Task<IEnumerable<Notification>> GetUnreadAsync(Guid userId);

    Task<IEnumerable<Notification>> GetAllAsync(Guid userId, int page = 1, int pageSize = 20);
}
