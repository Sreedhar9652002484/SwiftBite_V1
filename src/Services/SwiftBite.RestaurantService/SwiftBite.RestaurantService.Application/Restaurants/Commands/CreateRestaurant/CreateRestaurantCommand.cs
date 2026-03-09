using MediatR;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;

public record CreateRestaurantCommand(
    string OwnerId,
    string Name,
    string Description,
    string PhoneNumber,
    string Email,
    string Address,
    string City,
    string PinCode,
    double Latitude,
    double Longitude,
    CuisineType CuisineType,
    decimal MinimumOrderAmount,
    int AverageDeliveryTimeMinutes
) : IRequest<RestaurantDto>;