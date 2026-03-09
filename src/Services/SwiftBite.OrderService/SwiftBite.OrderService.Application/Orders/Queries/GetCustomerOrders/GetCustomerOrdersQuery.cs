using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetCustomerOrders;

public record GetCustomerOrdersQuery(string CustomerId)
    : IRequest<IEnumerable<OrderDto>>;