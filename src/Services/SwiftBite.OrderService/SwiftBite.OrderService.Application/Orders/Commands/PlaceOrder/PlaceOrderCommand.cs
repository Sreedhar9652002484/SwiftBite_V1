using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;

public record PlaceOrderCommand(
    string CustomerId,
    string CustomerName,
    string CustomerPhone,
    Guid RestaurantId,
    string RestaurantName,
    string DeliveryAddress,
    string DeliveryCity,
    string DeliveryPinCode,
    double DeliveryLatitude,
    double DeliveryLongitude,
    string PaymentMethod,
    string? SpecialInstructions,
    List<OrderItemRequest> Items
) : IRequest<OrderDto>;

public record OrderItemRequest(
    Guid MenuItemId,
    string Name,
    int Quantity,
    decimal UnitPrice,
    string? ImageUrl = null,
    string? Customization = null);