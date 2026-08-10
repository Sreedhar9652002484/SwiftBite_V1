using SwiftBite.UserService.Domain.Entities;

namespace SwiftBite.UserService.Domain.Interfaces;

public interface IAddressRepository
{
    Task<Address?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Address>> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<Address?> GetDefaultAsync(Guid userId, CancellationToken ct = default);
    Task AddAsync(Address address, CancellationToken ct = default);
    Task UpdateAsync(Address address, CancellationToken ct = default);
    Task DeleteAsync(Address address, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}