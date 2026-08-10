using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantsByCity;

public record GetRestaurantsByCityQuery(string City)
    : IRequest<IEnumerable<RestaurantDto>>;