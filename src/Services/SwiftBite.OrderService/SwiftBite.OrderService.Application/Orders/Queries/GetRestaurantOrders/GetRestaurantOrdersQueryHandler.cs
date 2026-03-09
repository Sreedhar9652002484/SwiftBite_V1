using MediatR;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetRestaurantOrders;

public class GetRestaurantOrdersQueryHandler
    : IRequestHandler<GetRestaurantOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetRestaurantOrdersQueryHandler(IOrderRepository repo)
        => _repo = repo;

    public async Task<IEnumerable<OrderDto>> Handle(
        GetRestaurantOrdersQuery query, CancellationToken ct)
    {
        var orders = await _repo
            .GetByRestaurantIdAsync(query.RestaurantId, ct);

        return orders
            .OrderByDescending(o => o.PlacedAt)
            .Select(PlaceOrderCommandHandler.MapToDto);
    }
}