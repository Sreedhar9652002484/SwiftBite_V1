using FluentValidation;

namespace SwiftBite.RestaurantService.Application.MenuCategories.Commands.CreateMenuCategory;

public class CreateMenuCategoryCommandValidator
    : AbstractValidator<CreateMenuCategoryCommand>
{
    public CreateMenuCategoryCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(50);
        RuleFor(x => x.DisplayOrder)
            .GreaterThanOrEqualTo(0);
    }
}