using MediatR;
using SwiftBite.RestaurantService.Application.MenuItems.DTOs;

namespace SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;

public record CreateMenuItemCommand(
    Guid CategoryId,
    Guid RestaurantId,
    string OwnerId,
    string Name,
    string Description,
    decimal Price,
    bool IsVegetarian,
    bool IsVegan,
    bool IsGlutenFree,
    int PreparationTimeMinutes,
    string? ImageUrl = null
) : IRequest<MenuItemDetailDto>;