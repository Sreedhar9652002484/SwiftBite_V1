using MediatR;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.ApproveRestaurant;

public record ApproveRestaurantCommand(Guid RestaurantId) : IRequest;
