using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Domain.Interfaces;

public interface IMenuItemRepository
{
    Task<MenuItem?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(Guid categoryId, CancellationToken ct = default);
    Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default);
    Task<IEnumerable<MenuItem>> SearchAsync(string keyword, CancellationToken ct = default);
    Task AddAsync(MenuItem item, CancellationToken ct = default);
    Task UpdateAsync(MenuItem item, CancellationToken ct = default);
    Task DeleteAsync(MenuItem item, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}