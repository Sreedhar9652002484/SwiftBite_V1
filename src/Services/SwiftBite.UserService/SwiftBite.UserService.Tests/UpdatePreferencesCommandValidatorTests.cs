using FluentValidation.TestHelper;
using SwiftBite.UserService.Application.Preferences.Commands.UpdatePreferences;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Tests;

public class UpdatePreferencesCommandValidatorTests
{
    private readonly UpdatePreferencesCommandValidator _validator = new();

    private static UpdatePreferencesCommand ValidCommand() => new(
        AuthUserId: "auth-1",
        DietaryPreference: DietaryPreference.Vegetarian,
        EmailNotifications: true,
        PushNotifications: false,
        SmsNotifications: true,
        PreferredCuisines: "Indian,Italian",
        AllergiesInfo: "None"
    );

    [Fact]
    public void Valid_Command_Has_No_Errors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_AuthUserId_Has_Error()
    {
        var command = ValidCommand() with { AuthUserId = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AuthUserId);
    }

    [Fact]
    public void Invalid_DietaryPreference_Has_Error()
    {
        var command = ValidCommand() with { DietaryPreference = (DietaryPreference)999 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DietaryPreference);
    }

    [Fact]
    public void Empty_PreferredCuisines_Has_Error()
    {
        var command = ValidCommand() with { PreferredCuisines = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PreferredCuisines);
    }

    [Fact]
    public void PreferredCuisines_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { PreferredCuisines = new string('a', 201) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PreferredCuisines);
    }

    [Fact]
    public void AllergiesInfo_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { AllergiesInfo = new string('a', 501) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.AllergiesInfo);
    }
}
