using Microsoft.EntityFrameworkCore;
using SwiftBite.UserService.Domain.Entities;
using SwiftBite.UserService.Domain.Interfaces;

namespace SwiftBite.UserService.Infrastructure.Persistence.Repositories;

public class AddressRepository : IAddressRepository
{
    private readonly UserDbContext _db;

    public AddressRepository(UserDbContext db) => _db = db;

    public async Task<Address?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.Addresses
            .FirstOrDefaultAsync(a => a.Id == id, ct);

    public async Task<IEnumerable<Address>> GetByUserIdAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.Addresses
            .Where(a => a.UserId == userId)
            .OrderByDescending(a => a.IsDefault)
            .ToListAsync(ct);

    public async Task<Address?> GetDefaultAsync(
        Guid userId, CancellationToken ct = default)
        => await _db.Addresses
            .FirstOrDefaultAsync(
                a => a.UserId == userId && a.IsDefault, ct);

    public async Task AddAsync(
        Address address, CancellationToken ct = default)
        => await _db.Addresses.AddAsync(address, ct);

    public Task UpdateAsync(
        Address address, CancellationToken ct = default)
    {
        _db.Addresses.Update(address);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        Address address, CancellationToken ct = default)
    {
        _db.Addresses.Remove(address);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}