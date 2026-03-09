using MediatR;

namespace SwiftBite.RestaurantService.Application.MenuItems.Commands.DeleteMenuItem;

public record DeleteMenuItemCommand(
    Guid ItemId,
    string OwnerId
) : IRequest<bool>;