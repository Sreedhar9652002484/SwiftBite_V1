using MediatR;
using SwiftBite.RestaurantService.Application.MenuCategories.DTOs;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;

public record CreateMenuCategoryCommand(
    Guid RestaurantId,
    string OwnerId,
    string Name,
    string? Description,
    int DisplayOrder
) : IRequest<MenuCategoryDto>;