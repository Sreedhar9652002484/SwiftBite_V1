using FluentValidation;

namespace SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;

public class UpdatePreferencesCommandValidator
    : AbstractValidator<UpdatePreferencesCommand>
{
    public UpdatePreferencesCommandValidator()
    {
        RuleFor(x => x.AuthUserId)
            .NotEmpty().WithMessage("AuthUserId is required.");

        RuleFor(x => x.DietaryPreference)
            .IsInEnum().WithMessage("Invalid dietary preference.");

        RuleFor(x => x.PreferredCuisines)
            .NotEmpty().MaximumLength(200);

        RuleFor(x => x.AllergiesInfo)
            .MaximumLength(500);
    }
}
