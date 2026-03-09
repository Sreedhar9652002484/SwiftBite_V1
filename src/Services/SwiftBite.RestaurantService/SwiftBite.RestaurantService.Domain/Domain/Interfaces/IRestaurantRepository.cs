using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Domain.Interfaces;

public interface IRestaurantRepository
{
    Task<Restaurant?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<Restaurant>> GetAllAsync(CancellationToken ct = default);
    Task<IEnumerable<Restaurant>> GetByCityAsync(string city, CancellationToken ct = default);
    Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(string ownerId, CancellationToken ct = default);
    Task AddAsync(Restaurant restaurant, CancellationToken ct = default);
    Task UpdateAsync(Restaurant restaurant, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}