using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetAllOrders;

public record GetAllOrdersQuery() : IRequest<IEnumerable<OrderDto>>;
