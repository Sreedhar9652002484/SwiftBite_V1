using Microsoft.EntityFrameworkCore;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly UserDbContext _db;

    public UserRepository(UserDbContext db) => _db = db;

    public async Task<User?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.Users
            .Include(u => u.Addresses)
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<User?> GetByAuthUserIdAsync(
        string authUserId, CancellationToken ct = default)
        => await _db.Users
            .Include(u => u.Addresses)
            .Include(u => u.Preference)
            .FirstOrDefaultAsync(u => u.AuthUserId == authUserId, ct);

    public async Task<User?> GetByEmailAsync(
        string email, CancellationToken ct = default)
        => await _db.Users
            .FirstOrDefaultAsync(u => u.Email == email, ct);

    public async Task<bool> ExistsAsync(
        string authUserId, CancellationToken ct = default)
        => await _db.Users
            .AnyAsync(u => u.AuthUserId == authUserId, ct);

    public async Task AddAsync(
        User user, CancellationToken ct = default)
        => await _db.Users.AddAsync(user, ct);

    public Task UpdateAsync(
        User user, CancellationToken ct = default)
    {
        _db.Users.Update(user);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}