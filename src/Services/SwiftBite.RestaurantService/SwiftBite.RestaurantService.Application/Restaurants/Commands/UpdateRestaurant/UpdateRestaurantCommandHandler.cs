using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.UpdateRestaurant;

public class UpdateRestaurantCommandHandler
    : IRequestHandler<UpdateRestaurantCommand, RestaurantDto>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public UpdateRestaurantCommandHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<RestaurantDto> Handle(
        UpdateRestaurantCommand cmd, CancellationToken ct)
    {
        var restaurant = await _repo.GetByIdAsync(cmd.Id, ct)
            ?? throw new KeyNotFoundException(
                $"Restaurant {cmd.Id} not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        restaurant.Update(
            cmd.Name, cmd.Description, cmd.PhoneNumber,
            cmd.Address, cmd.City, cmd.PinCode,
            cmd.MinimumOrderAmount,
            cmd.AverageDeliveryTimeMinutes, cmd.CuisineType);

        await _repo.UpdateAsync(restaurant, ct);
        await _repo.SaveChangesAsync(ct);

        // ✅ Invalidate caches
        await _cache.RemoveAsync($"restaurant:{cmd.Id}", ct);
        await _cache.RemoveByPrefixAsync(
            $"restaurants:city:{cmd.City.ToLower()}", ct);

        return CreateRestaurantCommandHandler.MapToDto(restaurant);
    }
}