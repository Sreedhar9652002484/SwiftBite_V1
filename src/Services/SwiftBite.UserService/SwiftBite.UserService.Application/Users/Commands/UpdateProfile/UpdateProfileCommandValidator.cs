using FluentValidation;

namespace SwiftBite.UserService.Application.Users.Commands.UpdateProfile;

public class UpdateProfileCommandValidator
    : AbstractValidator<UpdateProfileCommand>
{
    public UpdateProfileCommandValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().MaximumLength(50);

        RuleFor(x => x.PhoneNumber)
            .Matches(@"^\+?[1-9]\d{9,14}$")
            .When(x => !string.IsNullOrEmpty(x.PhoneNumber))
            .WithMessage("Invalid phone number format.");
    }
}