using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.MenuCategories.DTOs;
using SwiftBite.RestaurantService.Domain.Entities;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;

public class CreateMenuCategoryCommandHandler
    : IRequestHandler<CreateMenuCategoryCommand, MenuCategoryDto>
{
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly IMenuCategoryRepository _categoryRepo;
    private readonly ICacheService _cache;

    public CreateMenuCategoryCommandHandler(
        IRestaurantRepository restaurantRepo,
        IMenuCategoryRepository categoryRepo,
        ICacheService cache)
    {
        _restaurantRepo = restaurantRepo;
        _categoryRepo = categoryRepo;
        _cache = cache;
    }

    public async Task<MenuCategoryDto> Handle(
        CreateMenuCategoryCommand cmd, CancellationToken ct)
    {
        var restaurant = await _restaurantRepo
            .GetByIdAsync(cmd.RestaurantId, ct)
            ?? throw new KeyNotFoundException(
                "Restaurant not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        var category = MenuCategory.Create(
            cmd.RestaurantId, cmd.Name,
            cmd.Description, cmd.DisplayOrder);

        await _categoryRepo.AddAsync(category, ct);
        await _categoryRepo.SaveChangesAsync(ct);

        // ✅ Invalidate menu cache
        await _cache.RemoveAsync(
            $"menu:{cmd.RestaurantId}", ct);

        return new MenuCategoryDto
        {
            Id = category.Id,
            RestaurantId = category.RestaurantId,
            Name = category.Name,
            Description = category.Description,
            DisplayOrder = category.DisplayOrder,
            IsActive = category.IsActive
        };
    }
}