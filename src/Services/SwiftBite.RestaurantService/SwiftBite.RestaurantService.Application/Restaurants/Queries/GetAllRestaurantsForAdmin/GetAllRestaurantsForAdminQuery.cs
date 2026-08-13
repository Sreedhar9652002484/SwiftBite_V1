using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetAllRestaurantsForAdmin;

public record GetAllRestaurantsForAdminQuery() : IRequest<IEnumerable<RestaurantDto>>;
