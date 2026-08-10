using SwiftBite.NotificationService.Domain.Entities;

namespace SwiftBite.NotificationService.Domain.Interfaces;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(
        string userId, int page = 1, int pageSize = 20,
        CancellationToken ct = default);

    Task<int> GetUnreadCountAsync(
        string userId,
        CancellationToken ct = default);

    Task AddAsync(Notification notification,
        CancellationToken ct = default);

    Task MarkAllReadAsync(string userId,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}