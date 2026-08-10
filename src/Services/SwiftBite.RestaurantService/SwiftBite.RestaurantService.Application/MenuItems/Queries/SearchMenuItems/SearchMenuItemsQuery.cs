using MediatR;
using SwiftBite.RestaurantService.Application.MenuItems.DTOs;

namespace SwiftBite.RestaurantService.Application.MenuItems.Queries.SearchMenuItems;

public record SearchMenuItemsQuery(string Keyword)
    : IRequest<IEnumerable<MenuItemDetailDto>>;