using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetOrderById;

public record GetOrderByIdQuery(Guid OrderId)
    : IRequest<OrderDto>;