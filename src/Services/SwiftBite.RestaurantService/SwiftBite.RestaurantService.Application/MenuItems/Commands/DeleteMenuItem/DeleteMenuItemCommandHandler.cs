using MediatR;
using SwiftBite.RestaurantService.Application.Common.Interfaces;
using SwiftBite.RestaurantService.Domain.Interfaces;

namespace SwiftBite.RestaurantService.Application.MenuItems.Commands.DeleteMenuItem;

public class DeleteMenuItemCommandHandler
    : IRequestHandler<DeleteMenuItemCommand, bool>
{
    private readonly IMenuItemRepository _itemRepo;
    private readonly IRestaurantRepository _restaurantRepo;
    private readonly ICacheService _cache;

    public DeleteMenuItemCommandHandler(
        IMenuItemRepository itemRepo,
        IRestaurantRepository restaurantRepo,
        ICacheService cache)
    {
        _itemRepo = itemRepo;
        _restaurantRepo = restaurantRepo;
        _cache = cache;
    }

    public async Task<bool> Handle(
        DeleteMenuItemCommand cmd, CancellationToken ct)
    {
        var item = await _itemRepo.GetByIdAsync(cmd.ItemId, ct)
            ?? throw new KeyNotFoundException("Item not found.");

        var restaurant = await _restaurantRepo
            .GetByIdAsync(item.RestaurantId, ct)
            ?? throw new KeyNotFoundException(
                "Restaurant not found.");

        if (restaurant.OwnerId != cmd.OwnerId)
            throw new UnauthorizedAccessException(
                "You don't own this restaurant.");

        await _itemRepo.DeleteAsync(item, ct);
        await _itemRepo.SaveChangesAsync(ct);

        await _cache.RemoveAsync(
            $"menu:{item.RestaurantId}", ct);

        return true;
    }
}
