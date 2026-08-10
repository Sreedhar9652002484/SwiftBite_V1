using FluentValidation.TestHelper;
using SwiftBite.UserService.Application.Users.Commands.CreateUser;

namespace SwiftBite.UserService.Tests;

public class CreateUserCommandValidatorTests
{
    private readonly CreateUserCommandValidator _validator = new();

    private static CreateUserCommand ValidCommand() => new(
        AuthUserId: "auth-1",
        FirstName: "Jane",
        LastName: "Doe",
        Email: "jane.doe@example.com",
        DateOfBirth: new DateTime(2000, 1, 1)
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

    [Fact]
    public void Empty_Email_Has_Error()
    {
        var command = ValidCommand() with { Email = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_Email_Format_Has_Error()
    {
        var command = ValidCommand() with { Email = "not-an-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Under_13_Years_Old_Has_Error()
    {
        var command = ValidCommand() with { DateOfBirth = DateTime.UtcNow.AddYears(-5) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.DateOfBirth);
    }
}
