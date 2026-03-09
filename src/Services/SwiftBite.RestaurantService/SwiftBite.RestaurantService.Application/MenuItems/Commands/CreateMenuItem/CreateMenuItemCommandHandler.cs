using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.MenuItems.DTOs;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;

public class CreateMenuItemCommandHandler
    : IRequestHandler<CreateMenuItemCommand, MenuItemDetailDto>
{
    private readonly IMenuItemRepository _itemRepo;
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly ICacheService _cache;

    public CreateMenuItemCommandHandler(
        IMenuItemRepository itemRepo,
        IRestaurantRepository restaurantRepo,
        ICacheService cache)
    {
        _itemRepo = itemRepo;
        _restaurantRepo = restaurantRepo;
        _cache = cache;
    }

    public async Task<MenuItemDetailDto> Handle(
        CreateMenuItemCommand cmd, CancellationToken ct)
    {
        var restaurant = await _restaurantRepo
            .GetByIdAsync(cmd.RestaurantId, ct)
            ?? throw new KeyNotFoundException(
                "Restaurant not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        var item = MenuItem.Create(
            cmd.CategoryId, cmd.RestaurantId,
            cmd.Name, cmd.Description, cmd.Price,
            cmd.IsVegetarian, cmd.IsVegan,
            cmd.IsGlutenFree, cmd.PreparationTimeMinutes,
            cmd.ImageUrl);

        await _itemRepo.AddAsync(item, ct);
        await _itemRepo.SaveChangesAsync(ct);

        // ✅ Invalidate menu cache
        await _cache.RemoveAsync(
            $"menu:{cmd.RestaurantId}", ct);

        return MapToDto(item);
    }

    public static MenuItemDetailDto MapToDto(MenuItem i) => new()
    {
        Id = i.Id,
        CategoryId = i.CategoryId,
        RestaurantId = i.RestaurantId,
        Name = i.Name,
        Description = i.Description,
        Price = i.Price,
        ImageUrl = i.ImageUrl,
        IsVegetarian = i.IsVegetarian,
        IsVegan = i.IsVegan,
        IsGlutenFree = i.IsGlutenFree,
        IsBestseller = i.IsBestseller,
        Status = i.Status,
        PreparationTimeMinutes = i.PreparationTimeMinutes
    };
}