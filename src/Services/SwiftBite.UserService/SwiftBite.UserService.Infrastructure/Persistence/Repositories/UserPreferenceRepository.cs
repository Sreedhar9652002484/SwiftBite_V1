using Microsoft.EntityFrameworkCore;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Infrastructure.Persistence.Repositories;

public class UserPreferenceRepository : IUserPreferenceRepository
{
    private readonly UserDbContext _db;

    public UserPreferenceRepository(UserDbContext db) => _db = db;

    public async Task<UserPreference?> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.UserPreferences
            .FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(
        UserPreference preference, CancellationToken ct = default)
        => await _db.UserPreferences.AddAsync(preference, ct);

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}