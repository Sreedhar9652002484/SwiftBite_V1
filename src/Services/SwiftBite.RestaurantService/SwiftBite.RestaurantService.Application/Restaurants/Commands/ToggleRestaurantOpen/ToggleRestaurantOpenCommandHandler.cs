using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.ToggleRestaurantOpen;

public class ToggleRestaurantOpenCommandHandler
    : IRequestHandler<ToggleRestaurantOpenCommand, bool>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public ToggleRestaurantOpenCommandHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<bool> Handle(
        ToggleRestaurantOpenCommand cmd, CancellationToken ct)
    {
        var restaurant = await _repo.GetByIdAsync(cmd.RestaurantId, ct)
            ?? throw new KeyNotFoundException(
                $"Restaurant {cmd.RestaurantId} not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        restaurant.ToggleOpen();
        await _repo.UpdateAsync(restaurant, ct);
        await _repo.SaveChangesAsync(ct);

        await _cache.RemoveAsync(
            $"restaurant:{cmd.RestaurantId}", ct);

        return restaurant.IsOpen;
    }
}