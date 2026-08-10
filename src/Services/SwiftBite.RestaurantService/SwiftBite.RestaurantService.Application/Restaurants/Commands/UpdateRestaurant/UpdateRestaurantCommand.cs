using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.UpdateRestaurant;

public record UpdateRestaurantCommand(
    Guid Id,
    string OwnerId,
    string Name,
    string Description,
    string PhoneNumber,
    string Address,
    string City,
    string PinCode,
    decimal MinimumOrderAmount,
    int AverageDeliveryTimeMinutes,
    CuisineType CuisineType
) : IRequest<RestaurantDto>;