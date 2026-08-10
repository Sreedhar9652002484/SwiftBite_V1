using SwiftBite.UserService.Domain.Entities;

namespace SwiftBite.UserService.Domain.Interfaces;

public interface IUserPreferenceRepository
{
    Task<UserPreference?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(UserPreference preference, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}