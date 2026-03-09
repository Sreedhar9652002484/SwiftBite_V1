using MediatR;
using SwiftBite.RestaurantService.Application.MenuCategories.DTOs;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Queries.GetMenuByRestaurant;

public record GetMenuByRestaurantQuery(Guid RestaurantId)
    : IRequest<IEnumerable<MenuCategoryDto>>;