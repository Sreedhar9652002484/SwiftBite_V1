using MediatR;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetRestaurantOrders;

public record GetRestaurantOrdersQuery(Guid RestaurantId)
    : IRequest<IEnumerable<OrderDto>>;