using Microsoft.EntityFrameworkCore;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Repositories;

public class MenuCategoryRepository : IMenuCategoryRepository
{
    private readonly RestaurantDbContext _db;

    public MenuCategoryRepository(RestaurantDbContext db) => _db = db;

    public async Task<MenuCategory?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.MenuCategories
            .Include(c => c.MenuItems)
            .FirstOrDefaultAsync(c => c.Id == id, ct);

    public async Task<IEnumerable<MenuCategory>> GetByRestaurantIdAsync(
        Guid restaurantId, CancellationToken ct = default)
        => await _db.MenuCategories
            .Include(c => c.MenuItems)
            .Where(c => c.RestaurantId == restaurantId
                && c.IsActive)
            .OrderBy(c => c.DisplayOrder)
            .ToListAsync(ct);

    public async Task AddAsync(
        MenuCategory category, CancellationToken ct = default)
        => await _db.MenuCategories.AddAsync(category, ct);

    public Task UpdateAsync(
        MenuCategory category, CancellationToken ct = default)
    {
        _db.MenuCategories.Update(category);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        MenuCategory category, CancellationToken ct = default)
    {
        _db.MenuCategories.Remove(category);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}