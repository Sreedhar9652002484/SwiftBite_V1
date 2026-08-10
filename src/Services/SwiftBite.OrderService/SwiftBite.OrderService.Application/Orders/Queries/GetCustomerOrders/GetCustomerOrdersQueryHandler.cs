using MediatR;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetCustomerOrders;

public class GetCustomerOrdersQueryHandler
    : IRequestHandler<GetCustomerOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetCustomerOrdersQueryHandler(IOrderRepository repo)
        => _repo = repo;

    public async Task<IEnumerable<OrderDto>> Handle(
        GetCustomerOrdersQuery query, CancellationToken ct)
    {
        var orders = await _repo
            .GetByCustomerIdAsync(query.CustomerId, ct);

        return orders
            .OrderByDescending(o => o.PlacedAt)
            .Select(PlaceOrderCommandHandler.MapToDto);
    }
}