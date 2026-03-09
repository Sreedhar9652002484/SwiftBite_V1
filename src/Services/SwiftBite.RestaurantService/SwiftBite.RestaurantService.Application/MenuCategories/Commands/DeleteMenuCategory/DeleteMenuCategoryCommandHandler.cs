using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Commands.DeleteMenuCategory;

public class DeleteMenuCategoryCommandHandler
    : IRequestHandler<DeleteMenuCategoryCommand, bool>
{
    private readonly IMenuCategoryRepository _categoryRepo;
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly ICacheService _cache;

    public DeleteMenuCategoryCommandHandler(
        IMenuCategoryRepository categoryRepo,
        IRestaurantRepository restaurantRepo,
        ICacheService cache)
    {
        _categoryRepo = categoryRepo;
        _restaurantRepo = restaurantRepo;
        _cache = cache;
    }

    public async Task<bool> Handle(
        DeleteMenuCategoryCommand cmd, CancellationToken ct)
    {
        var category = await _categoryRepo
            .GetByIdAsync(cmd.CategoryId, ct)
            ?? throw new KeyNotFoundException(
                "Category not found.");

        var restaurant = await _restaurantRepo
            .GetByIdAsync(category.RestaurantId, ct)
            ?? throw new KeyNotFoundException(
                "Restaurant not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        await _categoryRepo.DeleteAsync(category, ct);
        await _categoryRepo.SaveChangesAsync(ct);

        await _cache.RemoveAsync(
            $"menu:{category.RestaurantId}", ct);

        return true;
    }
}