using MediatR;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Application.Events;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Entities;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;

public class PlaceOrderCommandHandler
    : IRequestHandler<PlaceOrderCommand, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly IEventPublisher _publisher;
    private readonly ICacheService _cache;


    public PlaceOrderCommandHandler(
        IOrderRepository repo,
        IEventPublisher publisher,
        ICacheService cache)
    {
        _repo = repo;
        _publisher = publisher;
        _cache = cache;
    }

    public async Task<OrderDto> Handle(
        PlaceOrderCommand cmd, CancellationToken ct)
    {
        // ✅ Build order items
        var items = cmd.Items.Select(i =>
            OrderItem.Create(
                i.MenuItemId, i.Name,
                i.Quantity, i.UnitPrice,
                i.ImageUrl, i.Customization))
            .ToList();

        // ✅ Create order
        var order = Order.Create(
            cmd.CustomerId, cmd.CustomerName,
            cmd.CustomerPhone, cmd.RestaurantId,
            cmd.RestaurantName, cmd.DeliveryAddress,
            cmd.DeliveryCity, cmd.DeliveryPinCode,
            cmd.DeliveryLatitude, cmd.DeliveryLongitude,
            cmd.PaymentMethod, cmd.SpecialInstructions,
            items);

        await _repo.AddAsync(order, ct);
        await _repo.SaveChangesAsync(ct);

        // 🔥 Publish to Kafka — swiftbite.order.placed
        await _publisher.PublishAsync(
            "swiftbite.order.placed",
            new OrderPlacedEvent
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = order.CustomerName,
                CustomerPhone = order.CustomerPhone,
                RestaurantId = order.RestaurantId,
                RestaurantName = order.RestaurantName,
                TotalAmount = order.TotalAmount,
                PaymentMethod = order.PaymentMethod,
                DeliveryAddress = order.DeliveryAddress,
                DeliveryCity = order.DeliveryCity,
                PlacedAt = order.PlacedAt,
                Items = order.Items.Select(i =>
                    new OrderItemEvent
                    {
                        MenuItemId = i.MenuItemId,
                        Name = i.Name,
                        Quantity = i.Quantity,
                        UnitPrice = i.UnitPrice,
                        TotalPrice = i.TotalPrice
                    }).ToList()
            }, ct);

        return MapToDto(order);
    }

    public static OrderDto MapToDto(Order o) => new()
    {
        Id = o.Id,
        CustomerId = o.CustomerId,
        CustomerName = o.CustomerName,
        RestaurantId = o.RestaurantId,
        RestaurantName = o.RestaurantName,
        DeliveryAddress = o.DeliveryAddress,
        DeliveryCity = o.DeliveryCity,
        SubTotal = o.SubTotal,
        DeliveryFee = o.DeliveryFee,
        Taxes = o.Taxes,
        Discount = o.Discount,
        TotalAmount = o.TotalAmount,
        Status = o.Status,
        PaymentStatus = o.PaymentStatus,
        PaymentMethod = o.PaymentMethod,
        SpecialInstructions = o.SpecialInstructions,
        PlacedAt = o.PlacedAt,
        EstimatedDeliveryAt = o.EstimatedDeliveryAt,
        DeliveredAt = o.DeliveredAt,
        RowVersion = o.RowVersion,
        Items = o.Items.Select(i => new OrderItemDto
        {
            Id = i.Id,
            MenuItemId = i.MenuItemId,
            Name = i.Name,
            ImageUrl = i.ImageUrl,
            Quantity = i.Quantity,
            UnitPrice = i.UnitPrice,
            TotalPrice = i.TotalPrice,
            Customization = i.Customization
        }).ToList(),
        StatusHistory = o.StatusHistory
            .OrderBy(h => h.Timestamp)
            .Select(h => new OrderStatusHistoryDto
            {
                Status = h.Status,
                Note = h.Note,
                Timestamp = h.Timestamp
            }).ToList()
    };
}