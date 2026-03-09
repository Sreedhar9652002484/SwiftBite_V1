using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Enums;

namespace SwiftBite.OrderService.Application.Orders.Commands.UpdateOrderStatus;

public record UpdateOrderStatusCommand(
    Guid OrderId,
    string RequesterId,  // Restaurant or Delivery partner
    OrderStatus NewStatus
) : IRequest<OrderDto>;