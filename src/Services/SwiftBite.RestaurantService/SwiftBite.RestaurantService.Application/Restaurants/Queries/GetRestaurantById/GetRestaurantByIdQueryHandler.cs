using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantById;

public class GetRestaurantByIdQueryHandler
    : IRequestHandler<GetRestaurantByIdQuery, RestaurantDto>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public GetRestaurantByIdQueryHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<RestaurantDto> Handle(
        GetRestaurantByIdQuery query, CancellationToken ct)
    {
        var cacheKey = $"restaurant:{query.Id}";

        // ⚡ Check cache first
        var cached = await _cache.GetAsync<RestaurantDto>(cacheKey, ct);
        if (cached != null) return cached;

        var restaurant = await _repo.GetByIdAsync(query.Id, ct)
            ?? throw new KeyNotFoundException(
                $"Restaurant {query.Id} not found.");

        var dto = CreateRestaurantCommandHandler.MapToDto(restaurant);

        // 💾 Cache for 5 minutes
        await _cache.SetAsync(cacheKey, dto, TimeSpan.FromMinutes(5), ct);

        return dto;
    }
}