using MediatR;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetAllOrders;

public class GetAllOrdersQueryHandler
    : IRequestHandler<GetAllOrdersQuery, IEnumerable<OrderDto>>
{
    private readonly IOrderRepository _repo;

    public GetAllOrdersQueryHandler(IOrderRepository repo)
        => _repo = repo;

    public async Task<IEnumerable<OrderDto>> Handle(
        GetAllOrdersQuery query, CancellationToken ct)
    {
        var orders = await _repo.GetAllAsync(ct);

        return orders
            .OrderByDescending(o => o.PlacedAt)
            .Select(PlaceOrderCommandHandler.MapToDto);
    }
}
