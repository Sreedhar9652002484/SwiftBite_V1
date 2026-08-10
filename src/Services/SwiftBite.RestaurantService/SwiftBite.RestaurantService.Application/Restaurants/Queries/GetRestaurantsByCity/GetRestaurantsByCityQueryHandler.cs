using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantsByCity;

public class GetRestaurantsByCityQueryHandler
    : IRequestHandler<GetRestaurantsByCityQuery, IEnumerable<RestaurantDto>>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public GetRestaurantsByCityQueryHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IEnumerable<RestaurantDto>> Handle(
        GetRestaurantsByCityQuery query, CancellationToken ct)
    {
        var cacheKey = $"restaurants:city:{query.City.ToLower()}";

        // ⚡ Cache first (heavy read — like Swiggy home page!)
        var cached = await _cache
            .GetAsync<IEnumerable<RestaurantDto>>(cacheKey, ct);
        if (cached != null) return cached;

        var restaurants = await _repo.GetByCityAsync(query.City, ct);
        var dtos = restaurants
            .Select(CreateRestaurantCommandHandler.MapToDto)
            .ToList();

        // 💾 Cache for 5 minutes
        await _cache.SetAsync(cacheKey, dtos,
            TimeSpan.FromMinutes(5), ct);

        return dtos;
    }
}