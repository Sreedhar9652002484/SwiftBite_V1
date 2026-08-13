using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetAllRestaurantsForAdmin;

public class GetAllRestaurantsForAdminQueryHandler
    : IRequestHandler<GetAllRestaurantsForAdminQuery, IEnumerable<RestaurantDto>>
{
    private readonly IRestaurantRepository _repo;

    public GetAllRestaurantsForAdminQueryHandler(IRestaurantRepository repo)
        => _repo = repo;

    public async Task<IEnumerable<RestaurantDto>> Handle(
        GetAllRestaurantsForAdminQuery query, CancellationToken ct)
    {
        var restaurants = await _repo.GetAllUnfilteredAsync(ct);

        return restaurants
            .Select(CreateRestaurantCommandHandler.MapToDto)
            .ToList();
    }
}
