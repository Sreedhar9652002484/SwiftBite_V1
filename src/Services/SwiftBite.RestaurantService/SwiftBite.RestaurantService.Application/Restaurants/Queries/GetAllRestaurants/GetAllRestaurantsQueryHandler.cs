using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetAllRestaurants;

public class GetAllRestaurantsQueryHandler
    : IRequestHandler<GetAllRestaurantsQuery, IEnumerable<RestaurantDto>>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public GetAllRestaurantsQueryHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<IEnumerable<RestaurantDto>> Handle(
        GetAllRestaurantsQuery query, CancellationToken ct)
    {
        var cacheKey = "restaurants:all";

        var cached = await _cache
            .GetAsync<IEnumerable<RestaurantDto>>(cacheKey, ct);
        if (cached != null) return cached;

        var restaurants = await _repo.GetAllAsync(ct);
        var dtos = restaurants
            .Select(CreateRestaurantCommandHandler.MapToDto)
            .ToList();

        await _cache.SetAsync(cacheKey, dtos,
            TimeSpan.FromMinutes(5), ct);

        return dtos;
    }
}