using Microsoft.EntityFrameworkCore;
using SwiftBite.NotificationService.Domain.Entities;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Infrastructure.Persistence.Repositories;

public class NotificationRepository
    : INotificationRepository
{
    private readonly NotificationDbContext _db;

    public NotificationRepository(
        NotificationDbContext db) => _db = db;

    public async Task<IEnumerable<Notification>>
        GetByUserIdAsync(
            string userId, int page = 1,
            int pageSize = 20,
            CancellationToken ct = default)
        => await _db.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(ct);

    public async Task<int> GetUnreadCountAsync(
        string userId, CancellationToken ct = default)
        => await _db.Notifications
            .CountAsync(n =>
                n.UserId == userId && !n.IsRead, ct);

    public async Task AddAsync(
        Notification notification,
        CancellationToken ct = default)
        => await _db.Notifications
            .AddAsync(notification, ct);

    public async Task MarkAllReadAsync(
        string userId, CancellationToken ct = default)
        => await _db.Notifications
            .Where(n =>
                n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s =>
                s.SetProperty(n => n.IsRead, true)
                 .SetProperty(n => n.ReadAt,
                    DateTime.UtcNow), ct);

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}