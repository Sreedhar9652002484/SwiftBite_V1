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
                .OrderBy(h => h.Timestamp))
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
        _db.Orders.Update(order);
        _db.Entry(order).State = Microsoft.EntityFrameworkCore.EntityState.Modified;
        return Task.CompletedTask;
        // ✅ FIX: Only call Update() if NOT already tracked
        //var entry = _db.Entry(order);

        //if (entry.State == EntityState.Detached)
        //{
        //    _db.Orders.Update(order);  
        //}
        // return Task.CompletedTask;

    }

    public async Task SaveChangesAsync(
        CancellationToken ct = default)
        => await _db.SaveChangesAsync(ct);

    public void SetOriginalRowVersion(Order order, byte[] rowVersion)
    {
        _db.Entry(order)
            .Property(o => o.RowVersion)
            .OriginalValue = rowVersion;
    }
}