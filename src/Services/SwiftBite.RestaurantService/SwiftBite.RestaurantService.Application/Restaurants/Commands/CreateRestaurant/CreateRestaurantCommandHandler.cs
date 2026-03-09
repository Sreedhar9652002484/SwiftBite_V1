using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.Restaurants.DTOs;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandHandler
    : IRequestHandler<CreateRestaurantCommand, RestaurantDto>
{
    private readonly IRestaurantRepository _repo;
    private readonly ICacheService _cache;

    public CreateRestaurantCommandHandler(
        IRestaurantRepository repo, ICacheService cache)
    {
        _repo = repo;
        _cache = cache;
    }

    public async Task<RestaurantDto> Handle(
        CreateRestaurantCommand cmd, CancellationToken ct)
    {
        var restaurant = Restaurant.Create(
            cmd.OwnerId, cmd.Name, cmd.Description,
            cmd.PhoneNumber, cmd.Email, cmd.Address,
            cmd.City, cmd.PinCode, cmd.Latitude,
            cmd.Longitude, cmd.CuisineType,
            cmd.MinimumOrderAmount,
            cmd.AverageDeliveryTimeMinutes);

        await _repo.AddAsync(restaurant, ct);
        await _repo.SaveChangesAsync(ct);

        // ✅ Invalidate city cache
        await _cache.RemoveByPrefixAsync(
            $"restaurants:city:{cmd.City.ToLower()}", ct);

        return MapToDto(restaurant);
    }

    public static RestaurantDto MapToDto(Restaurant r) => new()
    {
        Id = r.Id,
        Name = r.Name,
        Description = r.Description,
        PhoneNumber = r.PhoneNumber,
        Email = r.Email,
        Address = r.Address,
        City = r.City,
        PinCode = r.PinCode,
        Latitude = r.Latitude,
        Longitude = r.Longitude,
        LogoUrl = r.LogoUrl,
        BannerUrl = r.BannerUrl,
        CuisineType = r.CuisineType,
        Status = r.Status,
        AverageRating = r.AverageRating,
        TotalRatings = r.TotalRatings,
        MinimumOrderAmount = r.MinimumOrderAmount,
        AverageDeliveryTimeMinutes = r.AverageDeliveryTimeMinutes,
        IsOpen = r.IsOpen,
        CreatedAt = r.CreatedAt
    };
}