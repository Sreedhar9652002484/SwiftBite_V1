using FluentValidation;

namespace SwiftBite.UserService.Application.Addresses.Commands.AddAddress;

public class AddAddressCommandValidator
    : AbstractValidator<AddAddressCommand>
{
    public AddAddressCommandValidator()
    {
        RuleFor(x => x.Label).NotEmpty().MaximumLength(30);
        RuleFor(x => x.FullAddress).NotEmpty().MaximumLength(250);
        RuleFor(x => x.Street).NotEmpty().MaximumLength(100);
        RuleFor(x => x.City).NotEmpty().MaximumLength(50);
        RuleFor(x => x.State).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PinCode)
            .NotEmpty()
            .Matches(@"^\d{6}$")
            .WithMessage("PinCode must be 6 digits.");
        RuleFor(x => x.Latitude)
            .InclusiveBetween(-90, 90);
        RuleFor(x => x.Longitude)
            .InclusiveBetween(-180, 180);
    }
}