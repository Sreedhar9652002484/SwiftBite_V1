using MediatR;
using SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;
using SwiftBite.RestaurantService.Application.MenuItems.DTOs;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuItems.Queries.SearchMenuItems;

public class SearchMenuItemsQueryHandler
    : IRequestHandler<SearchMenuItemsQuery, IEnumerable<MenuItemDetailDto>>
{
    private readonly IMenuItemRepository _itemRepo;

    public SearchMenuItemsQueryHandler(IMenuItemRepository itemRepo)
        => _itemRepo = itemRepo;

    public async Task<IEnumerable<MenuItemDetailDto>> Handle(
        SearchMenuItemsQuery query, CancellationToken ct)
    {
        var items = await _itemRepo.SearchAsync(query.Keyword, ct);
        return items.Select(CreateMenuItemCommandHandler.MapToDto);
    }
}