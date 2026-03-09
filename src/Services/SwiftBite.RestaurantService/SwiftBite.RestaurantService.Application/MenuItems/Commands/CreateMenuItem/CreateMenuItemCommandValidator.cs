using FluentValidation;

namespace SwiftBite.RestaurantService.Application.MenuItems.Commands.CreateMenuItem;

public class CreateMenuItemCommandValidator
    : AbstractValidator<CreateMenuItemCommand>
{
    public CreateMenuItemCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(500);
        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");
        RuleFor(x => x.PreparationTimeMinutes)
            .InclusiveBetween(1, 120);
    }
}