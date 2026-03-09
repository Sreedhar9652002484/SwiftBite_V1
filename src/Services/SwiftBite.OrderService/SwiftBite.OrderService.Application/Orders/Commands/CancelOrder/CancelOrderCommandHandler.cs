using MediatR;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Application.Events;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Commands.CancelOrder;

public class CancelOrderCommandHandler
    : IRequestHandler<CancelOrderCommand, bool>
{
    private readonly IOrderRepository _repo;
    private readonly IEventPublisher _publisher;
    private readonly ICacheService _cache;

    public CancelOrderCommandHandler(
        IOrderRepository repo,
        IEventPublisher publisher,
        ICacheService cache)
    {
        _repo = repo;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<bool> Handle(
        CancelOrderCommand cmd, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new KeyNotFoundException(
                $"Order {cmd.OrderId} not found.");

        if (order.CustomerId != cmd.CustomerId)
            throw new UnauthorizedAccessException(
                "You don't own this order.");

        order.Cancel(cmd.Reason);

        await _repo.UpdateAsync(order, ct);
        await _repo.SaveChangesAsync(ct);

        // ✅ Invalidate cache
        await _cache.RemoveAsync(
            $"order:{cmd.OrderId}", ct);

        // 🔥 Publish to Kafka — swiftbite.order.cancelled
        await _publisher.PublishAsync(
            "swiftbite.order.cancelled",
            new OrderCancelledEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                RestaurantId = order.RestaurantId,
                RefundAmount = order.TotalAmount,
                Reason = cmd.Reason,
                CancelledAt = DateTime.UtcNow
            }, ct);

        return true;
    }
}