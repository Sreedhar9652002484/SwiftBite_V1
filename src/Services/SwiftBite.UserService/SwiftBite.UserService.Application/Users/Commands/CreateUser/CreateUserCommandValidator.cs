using FluentValidation;

namespace SwiftBite.UserService.Application.Users.Commands.CreateUser;

public class CreateUserCommandValidator
    : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.AuthUserId)
            .NotEmpty().WithMessage("AuthUserId is required.");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MaximumLength(50);

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MaximumLength(50);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress().WithMessage("Valid email is required.");

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow.AddYears(-13))
            .WithMessage("User must be at least 13 years old.");
    }
}