using FluentValidation;

namespace SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandValidator
    : AbstractValidator<CreateRestaurantCommand>
{
    public CreateRestaurantCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty().MaximumLength(100);
        RuleFor(x => x.Description)
            .NotEmpty().MaximumLength(500);
        RuleFor(x => x.PhoneNumber)
            .NotEmpty().Matches(@"^\+?[1-9]\d{9,14}$");
        RuleFor(x => x.Email)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.Address)
            .NotEmpty().MaximumLength(250);
        RuleFor(x => x.City)
            .NotEmpty().MaximumLength(50);
        RuleFor(x => x.PinCode)
            .NotEmpty().Matches(@"^\d{6}$");
        RuleFor(x => x.MinimumOrderAmount)
            .GreaterThan(0);
        RuleFor(x => x.AverageDeliveryTimeMinutes)
            .InclusiveBetween(10, 120);
    }
}