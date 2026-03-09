using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Application.MenuCategories.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Queries.GetMenuByRestaurant;

public class GetMenuByRestaurantQueryHandler
    : IRequestHandler<GetMenuByRestaurantQuery, IEnumerable<MenuCategoryDto>>
{
    private readonly IMenuCategoryRepository _categoryRepo;
    private readonly ICacheService _cache;

    public GetMenuByRestaurantQueryHandler(
        IMenuCategoryRepository categoryRepo,
        ICacheService cache)
    {
        _categoryRepo = categoryRepo;
        _cache = cache;
    }

    public async Task<IEnumerable<MenuCategoryDto>> Handle(
        GetMenuByRestaurantQuery query, CancellationToken ct)
    {
        var cacheKey = $"menu:{query.RestaurantId}";

        // ⚡ Cache first — menus are heavily read!
        var cached = await _cache
            .GetAsync<IEnumerable<MenuCategoryDto>>(cacheKey, ct);
        if (cached != null) return cached;

        var categories = await _categoryRepo
            .GetByRestaurantIdAsync(query.RestaurantId, ct);

        var dtos = categories.Select(c => new MenuCategoryDto
        {
            Id = c.Id,
            RestaurantId = c.RestaurantId,
            Name = c.Name,
            Description = c.Description,
            DisplayOrder = c.DisplayOrder,
            IsActive = c.IsActive,
            MenuItems = c.MenuItems.Select(i => new MenuItemDto
            {
                Id = i.Id,
                Name = i.Name,
                Description = i.Description,
                Price = i.Price,
                ImageUrl = i.ImageUrl,
                IsVegetarian = i.IsVegetarian,
                IsVegan = i.IsVegan,
                IsGlutenFree = i.IsGlutenFree,
                IsBestseller = i.IsBestseller,
                PreparationTimeMinutes = i.PreparationTimeMinutes,
                Status = i.Status.ToString()
            }).ToList()
        }).OrderBy(c => c.DisplayOrder).ToList();

        // 💾 Cache for 10 minutes (menus rarely change!)
        await _cache.SetAsync(cacheKey, dtos,
            TimeSpan.FromMinutes(10), ct);

        return dtos;
    }
}