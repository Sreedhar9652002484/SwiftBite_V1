using MediatR;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.ToggleRestaurantOpen;

public record ToggleRestaurantOpenCommand(
    Guid RestaurantId,
    string OwnerId
) : IRequest<bool>;