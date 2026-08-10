using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Domain.Interfaces;

public interface IMenuCategoryRepository
{
    Task<MenuCategory?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IEnumerable<MenuCategory>> GetByRestaurantIdAsync(Guid restaurantId, CancellationToken ct = default);
    Task AddAsync(MenuCategory category, CancellationToken ct = default);
    Task UpdateAsync(MenuCategory category, CancellationToken ct = default);
    Task DeleteAsync(MenuCategory category, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}