using FluentValidation.TestHelper;
using SwiftBite.UserService.Application.Users.Commands.UpdateProfile;

namespace SwiftBite.UserService.Tests;

public class UpdateProfileCommandValidatorTests
{
    private readonly UpdateProfileCommandValidator _validator = new();

    private static UpdateProfileCommand ValidCommand() => new(
        AuthUserId: "auth-1",
        FirstName: "Jane",
        LastName: "Doe",
        PhoneNumber: "+919876543210",
        ProfilePictureUrl: "https://example.com/pic.jpg"
    );

    [Fact]
    public void Valid_Command_Has_No_Errors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Null_Optional_Fields_Have_No_Errors()
    {
        var command = ValidCommand() with { PhoneNumber = null, ProfilePictureUrl = null };
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_FirstName_Has_Error()
    {
        var command = ValidCommand() with { FirstName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void FirstName_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { FirstName = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FirstName);
    }

    [Fact]
    public void Empty_LastName_Has_Error()
    {
        var command = ValidCommand() with { LastName = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Fact]
    public void LastName_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { LastName = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.LastName);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("abc123")]
    [InlineData("0123456789")]
    public void Invalid_PhoneNumber_Format_Has_Error(string phoneNumber)
    {
        var command = ValidCommand() with { PhoneNumber = phoneNumber };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }
}
