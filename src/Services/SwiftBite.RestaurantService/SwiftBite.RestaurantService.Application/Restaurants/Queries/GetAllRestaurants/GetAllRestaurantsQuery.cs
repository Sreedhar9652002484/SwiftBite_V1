using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetAllRestaurants;

public record GetAllRestaurantsQuery() : IRequest<IEnumerable<RestaurantDto>>;