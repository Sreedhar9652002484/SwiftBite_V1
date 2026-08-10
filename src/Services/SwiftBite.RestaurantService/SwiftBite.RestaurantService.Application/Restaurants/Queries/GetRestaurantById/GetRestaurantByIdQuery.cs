using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;

namespace SwiftBite.RestaurantService.Application.Restaurants.Queries.GetRestaurantById;

public record GetRestaurantByIdQuery(Guid Id)
    : IRequest<RestaurantDto>;