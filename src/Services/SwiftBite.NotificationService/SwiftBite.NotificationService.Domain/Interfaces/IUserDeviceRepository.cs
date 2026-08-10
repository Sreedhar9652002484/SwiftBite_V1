using SwiftBite.NotificationService.Domain.Entities;

namespace SwiftBite.NotificationService.Domain.Interfaces;

public interface IUserDeviceRepository
{
    Task<IEnumerable<UserDevice>> GetByUserIdAsync(
        string userId,
        CancellationToken ct = default);

    Task<UserDevice?> GetByTokenAsync(
        string deviceToken,
        CancellationToken ct = default);

    Task AddAsync(UserDevice device,
        CancellationToken ct = default);

    Task UpdateAsync(UserDevice device,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}