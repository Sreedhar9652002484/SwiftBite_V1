using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Application.Events;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Enums;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Commands.UpdateOrderStatus;

public class UpdateOrderStatusCommandHandler
    : IRequestHandler<UpdateOrderStatusCommand, OrderDto>
{
   
    private readonly IOrderRepository _repo;
    private readonly IEventPublisher _publisher;
    private readonly ICacheService _cache;

    public UpdateOrderStatusCommandHandler(
        IOrderRepository repo,
        IEventPublisher publisher,
        ICacheService cache)
    {
        _repo = repo;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<OrderDto> Handle(
        UpdateOrderStatusCommand cmd, CancellationToken ct)
    {
        var order = await _repo.GetByIdAsync(cmd.OrderId, ct)
            ?? throw new KeyNotFoundException(
                $"Order {cmd.OrderId} not found.");

        // ✅🔥 CRITICAL LINE (YOU WERE MISSING THIS)
        _repo.SetOriginalRowVersion(order, cmd.RowVersion);
        // ✅ Transition to new status
        switch (cmd.NewStatus)
        {
            case OrderStatus.Confirmed:
                order.Confirm();
                // 🔥 Kafka event
                await _publisher.PublishAsync(
                    "swiftbite.order.confirmed",
                    new OrderConfirmedEvent
                    {
                        OrderId = order.Id,
                        RestaurantId = order.RestaurantId,
                        CustomerId = order.CustomerId,
                        EstimatedPrepTimeMinutes = 30,
                        ConfirmedAt = DateTime.UtcNow
                    }, ct);
                break;

            case OrderStatus.Preparing:
                order.StartPreparing();
                break;

            case OrderStatus.Ready:
                order.MarkReady();
                break;

            case OrderStatus.PickedUp:
                order.MarkPickedUp();
                break;

            case OrderStatus.OutForDelivery:
                order.MarkOutForDelivery();
                break;

            case OrderStatus.Delivered:
                order.MarkDelivered();
                break;

            default:
                throw new InvalidOperationException(
                    $"Invalid status transition: {cmd.NewStatus}");
        }

        await _repo.UpdateAsync(order, ct);
        // 4️⃣ SAVE WITH ERROR HANDLING
        try
        {
            await _repo.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException(
                $"Failed to update order {cmd.OrderId}. The order may have been modified or deleted.",
                ex);
        }
        catch (DbUpdateException ex)
        {
            throw new InvalidOperationException(
                $"Database error while updating order {cmd.OrderId}.",
                ex);
        }


        // ✅ Invalidate cache
        await _cache.RemoveAsync($"order:{cmd.OrderId}", ct);

        return PlaceOrderCommandHandler.MapToDto(order);
    }
}