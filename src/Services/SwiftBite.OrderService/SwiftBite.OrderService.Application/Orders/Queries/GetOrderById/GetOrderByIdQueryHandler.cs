using MediatR;
using SwiftBite.OrderService.Application.Common.Interfaces;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;
using SwiftBite.OrderService.Domain.Interfaces;

namespace SwiftBite.OrderService.Application.Orders.Queries.GetOrderById;

public class GetOrderByIdQueryHandler
    : IRequestHandler<GetOrderByIdQuery, OrderDto>
{
    private readonly IOrderRepository _repo;
    private readonly ICacheService _cache;

    public GetOrderByIdQueryHandler(
        IOrderRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<OrderDto> Handle(
        GetOrderByIdQuery query, CancellationToken ct)
    {
        var cacheKey = $"order:{query.OrderId}";

        // ⚡ Cache first
        var cached = await _cache
            .GetAsync<OrderDto>(cacheKey, ct);
        if (cached != null) return cached;

        var order = await _repo
            .GetByIdAsync(query.OrderId, ct)
            ?? throw new KeyNotFoundException(
                $"Order {query.OrderId} not found.");

        var dto = PlaceOrderCommandHandler.MapToDto(order);

        // 💾 Cache for 2 minutes
        await _cache.SetAsync(cacheKey, dto,
            TimeSpan.FromMinutes(2), ct);

        return dto;
    }
}