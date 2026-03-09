using MediatR;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Commands.DeleteMenuCategory;

public record DeleteMenuCategoryCommand(
    Guid CategoryId,
    string OwnerId
) : IRequest<bool>;