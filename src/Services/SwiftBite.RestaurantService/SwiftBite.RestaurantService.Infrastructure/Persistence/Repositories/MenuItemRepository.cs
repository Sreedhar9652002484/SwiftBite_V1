using Microsoft.EntityFrameworkCore;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Repositories;

public class MenuItemRepository : IMenuItemRepository
{
    private readonly RestaurantDbContext _db;

    public MenuItemRepository(RestaurantDbContext db) => _db = db;

    public async Task<MenuItem?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.MenuItems
            .FirstOrDefaultAsync(i => i.Id == id, ct);

    public async Task<IEnumerable<MenuItem>> GetByCategoryIdAsync(
        Guid categoryId, CancellationToken ct = default)
        => await _db.MenuItems
            .Where(i => i.CategoryId == categoryId)
            .ToListAsync(ct);

    public async Task<IEnumerable<MenuItem>> GetByRestaurantIdAsync(
        Guid restaurantId, CancellationToken ct = default)
        => await _db.MenuItems
            .Where(i => i.RestaurantId == restaurantId)
            .ToListAsync(ct);

    public async Task<IEnumerable<MenuItem>> SearchAsync(
        string keyword, CancellationToken ct = default)
        => await _db.MenuItems
            .Where(i => i.Name.Contains(keyword)
                || i.Description.Contains(keyword))
            .ToListAsync(ct);

    public async Task AddAsync(
        MenuItem item, CancellationToken ct = default)
        => await _db.MenuItems.AddAsync(item, ct);

    public Task UpdateAsync(
        MenuItem item, CancellationToken ct = default)
    {
        _db.MenuItems.Update(item);
        return Task.CompletedTask;
    }

    public Task DeleteAsync(
        MenuItem item, CancellationToken ct = default)
    {
        _db.MenuItems.Remove(item);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}