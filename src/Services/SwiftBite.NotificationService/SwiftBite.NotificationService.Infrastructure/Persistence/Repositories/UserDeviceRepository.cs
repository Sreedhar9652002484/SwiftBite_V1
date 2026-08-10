using Microsoft.EntityFrameworkCore;
using SwiftBite.NotificationService.Domain.Entities;
using SwiftBite.NotificationService.Domain.Interfaces;

namespace SwiftBite.NotificationService.Infrastructure.Persistence.Repositories;

public class UserDeviceRepository : IUserDeviceRepository
{
    private readonly NotificationDbContext _db;

    public UserDeviceRepository(
        NotificationDbContext db) => _db = db;

    public async Task<IEnumerable<UserDevice>>
        GetByUserIdAsync(
            string userId,
            CancellationToken ct = default)
        => await _db.UserDevices
            .Where(d =>
                d.UserId == userId && d.IsActive)
            .ToListAsync(ct);

    public async Task<UserDevice?> GetByTokenAsync(
        string deviceToken,
        CancellationToken ct = default)
        => await _db.UserDevices
            .FirstOrDefaultAsync(
                d => d.DeviceToken == deviceToken, ct);

    public async Task AddAsync(
        UserDevice device,
        CancellationToken ct = default)
        => await _db.UserDevices.AddAsync(device, ct);

    public Task UpdateAsync(
        UserDevice device,
        CancellationToken ct = default)
    {
        _db.UserDevices.Update(device);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}