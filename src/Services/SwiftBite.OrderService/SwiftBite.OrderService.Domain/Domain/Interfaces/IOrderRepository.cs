using SwiftBite.OrderService.Domain.Entities;
using SwiftBite.OrderService.Domain.Enums;

namespace SwiftBite.OrderService.Domain.Interfaces;

public interface IOrderRepository
{
    Task<Order?> GetByIdAsync(Guid id,
        CancellationToken ct = default);

    Task<IEnumerable<Order>> GetByCustomerIdAsync(
        string customerId,
        CancellationToken ct = default);

    Task<IEnumerable<Order>> GetByRestaurantIdAsync(
        Guid restaurantId,
        CancellationToken ct = default);

    Task<IEnumerable<Order>> GetByStatusAsync(
        OrderStatus status,
        CancellationToken ct = default);

    Task AddAsync(Order order,
        CancellationToken ct = default);

    Task UpdateAsync(Order order,
        CancellationToken ct = default);

    Task SaveChangesAsync(
        CancellationToken ct = default);
}