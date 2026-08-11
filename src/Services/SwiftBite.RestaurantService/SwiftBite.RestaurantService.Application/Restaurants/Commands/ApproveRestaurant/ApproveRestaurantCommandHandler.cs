using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.ApproveRestaurant;

public class ApproveRestaurantCommandHandler : IRequestHandler<ApproveRestaurantCommand>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public ApproveRestaurantCommandHandler(IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task Handle(ApproveRestaurantCommand cmd, CancellationToken ct)
    {
        var restaurant = await _repo.GetByIdAsync(cmd.RestaurantId, ct)
            ?? throw new KeyNotFoundException($"Restaurant {cmd.RestaurantId} not found.");

        restaurant.Approve();
        await _repo.UpdateAsync(restaurant, ct);
        await _repo.SaveChangesAsync(ct);

        await _cache.RemoveAsync($"restaurant:{cmd.RestaurantId}", ct);
        await _cache.RemoveByPrefixAsync($"restaurants:city:{restaurant.City.ToLower()}", ct);
        await _cache.RemoveAsync("restaurants:all", ct);
    }
}
