using MediatR;
using Microsoft.EntityFrameworkCore;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Application.Events;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Enums;
using SwiftBite.OrderService.Domain.Interfaces;
using SwiftBite.Shared.Kernel.Events;

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
      
        // ✅ Transition to new status
        switch (cmd.NewStatus)
        {
            case OrderStatus.Confirmed: order.Confirm(); break;
            case OrderStatus.Preparing: order.StartPreparing(); break;
            case OrderStatus.Ready: order.MarkReady(); break;
            case OrderStatus.PickedUp: order.MarkPickedUp(); break;
            case OrderStatus.OutForDelivery: order.MarkOutForDelivery(); break;
            case OrderStatus.Delivered: order.MarkDelivered(); break;
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
            var dbEntry = ex.Entries.FirstOrDefault();
            var failingEntityName = dbEntry?.Entity.GetType().Name ?? "Unknown";

            var dbValues = dbEntry != null
                ? await dbEntry.GetDatabaseValuesAsync(ct)
                : null;

            if (dbValues == null)
                throw new InvalidOperationException(
                    $"{failingEntityName} was not found in the database. EF Core tracking failed.");

            throw new InvalidOperationException(
                $"Order {cmd.OrderId} was modified by another process. Please refresh and retry.");
        }
        // 🔥 Kafka event
        if (cmd.NewStatus == OrderStatus.Confirmed)
        {
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
        }

        // ✅ After SaveChangesAsync succeeds, publish event:
        if (cmd.NewStatus == OrderStatus.Ready)
        {
            await _publisher.PublishAsync(
                "swiftbite.order.ready",
                new OrderReadyEvent
                {
                    OrderId = order.Id,
                    CustomerId = order.CustomerId,  // ✅ ADD
                    OrderNumber = order.Id.ToString()[..8].ToUpper(),

                    RestaurantId = order.RestaurantId,
                    RestaurantName = order.RestaurantName,
                    CustomerName = order.CustomerName,
                    CustomerPhone = order.CustomerPhone,
                    DeliveryAddress = order.DeliveryAddress,
                    DeliveryCity = order.DeliveryCity,
                    DeliveryPinCode = order.DeliveryPinCode,
                    DeliveryLatitude = order.DeliveryLatitude,
                    DeliveryLongitude = order.DeliveryLongitude,
                    DeliveryFee = order.DeliveryFee,
                    ReadyAt = DateTime.UtcNow
                }, ct);
        }

        // ✅ Invalidate cache
        await _cache.RemoveAsync($"order:{cmd.OrderId}", ct);

        return PlaceOrderCommandHandler.MapToDto(order);
    }
}