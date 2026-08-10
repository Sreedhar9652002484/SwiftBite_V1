using Microsoft.EntityFrameworkCore;
using SwiftBite.OrderService.Domain.Entities;
using SwiftBite.OrderService.Domain.Enums;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly OrderDbContext _db;

    public OrderRepository(OrderDbContext db) => _db = db;

    public async Task<Order?> GetByIdAsync(
        Guid id, CancellationToken ct = default)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory
                //.OrderBy(h => h.Timestamp)
            )
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<IEnumerable<Order>> GetByCustomerIdAsync(
        string customerId, CancellationToken ct = default)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Where(o => o.CustomerId == customerId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByRestaurantIdAsync(
        Guid restaurantId, CancellationToken ct = default)
        => await _db.Orders
            .Include(o => o.Items)
            .Include(o => o.StatusHistory)
            .Where(o => o.RestaurantId == restaurantId)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(ct);

    public async Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status, CancellationToken ct = default)
        => await _db.Orders
            .Include(o => o.Items)
            .Where(o => o.Status == status)
            .OrderByDescending(o => o.PlacedAt)
            .ToListAsync(ct);

    public async Task AddAsync(
        Order order, CancellationToken ct = default)
        => await _db.Orders.AddAsync(order, ct);

    public Task UpdateAsync(
        Order order, CancellationToken ct = default)
    {
        //_db.Orders.Update(order);
        foreach (var history in order.StatusHistory)
        {
            var entry = _db.Entry(history);

            // If EF Core is confused and thinks this is an existing record being updated:
            if (entry.State == EntityState.Modified)
            {
                entry.State = EntityState.Added;
            }
        }
        return Task.CompletedTask;

    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public void SetOriginalRowVersion(Order order, string rowVersionBase64)
    {
        var rowVersionBytes = Convert.FromBase64String(rowVersionBase64);

        var entry = _db.Entry(order);

        // Log what EF currently has vs what client sent
        var currentOriginal = entry.Property(o => o.RowVersion).OriginalValue;
        Console.WriteLine($"[RowVersion] DB original: {Convert.ToBase64String((byte[])currentOriginal)}");
        Console.WriteLine($"[RowVersion] Client sent: {rowVersionBase64}");

        entry.State = EntityState.Unchanged;
        entry.Property(o => o.RowVersion).OriginalValue = rowVersionBytes;
        entry.State = EntityState.Modified;

        Console.WriteLine($"[RowVersion] Applied successfully");
    }
}