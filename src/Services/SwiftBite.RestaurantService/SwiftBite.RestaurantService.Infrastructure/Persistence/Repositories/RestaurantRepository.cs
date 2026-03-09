using Microsoft.EntityFrameworkCore;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Repositories;

public class RestaurantRepository : IRestaurantRepository
{
    private readonly RestaurantDbContext _db;

    public RestaurantRepository(RestaurantDbContext db) => _db = db;

    public async Task<Restaurant?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.Restaurants
            .Include(r => r.MenuCategories)
                .ThenInclude(c => c.MenuItems)
            .Include(r => r.Hours)
            .FirstOrDefaultAsync(r => r.Id == id, ct);

    public async Task<IEnumerable<Restaurant>> GetAllAsync(
        CancellationToken ct = default)
        => await _db.Restaurants
            .Where(r => r.Status ==
                Domain.Enums.RestaurantStatus.Active)
            .OrderByDescending(r => r.AverageRating)
            .ToListAsync(ct);

    public async Task<IEnumerable<Restaurant>> GetByCityAsync(
        string city, CancellationToken ct = default)
        => await _db.Restaurants
            .Where(r => r.City.ToLower() == city.ToLower()
                && r.Status == Domain.Enums.RestaurantStatus.Active)
            .OrderByDescending(r => r.AverageRating)
            .ToListAsync(ct);

    public async Task<IEnumerable<Restaurant>> GetByOwnerIdAsync(
        string ownerId, CancellationToken ct = default)
        => await _db.Restaurants
            .Where(r => r.OwnerId == ownerId)
            .ToListAsync(ct);

    public async Task AddAsync(
        Restaurant restaurant, CancellationToken ct = default)
        => await _db.Restaurants.AddAsync(restaurant, ct);

    public Task UpdateAsync(
        Restaurant restaurant, CancellationToken ct = default)
    {
        _db.Restaurants.Update(restaurant);
        return Task.CompletedTask;
    }

    public async Task SaveChangesAsync(CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);
}