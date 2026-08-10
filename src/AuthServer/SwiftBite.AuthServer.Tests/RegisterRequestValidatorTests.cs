using FluentValidation.TestHelper;
using SwiftBite.AuthServer.Models;
using SwiftBite.AuthServer.Models.Validators;

namespace SwiftBite.AuthServer.Tests;

public class RegisterRequestValidatorTests
{
    private readonly RegisterRequestValidator _validator = new();

    private static RegisterRequest ValidRequest() => new()
    {
        FirstName = "Jane",
        LastName = "Doe",
        Email = "jane.doe@example.com",
        Password = "Passw0rd!",
        ConfirmPassword = "Passw0rd!"
    };

    [Fact]
    public void Valid_request_passes()
    {
        var result = _validator.TestValidate(ValidRequest());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var request = ValidRequest();
        request.Email = "";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var request = ValidRequest();
        request.Email = "not-an-email";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Theory]
    [InlineData("short1A")]      // too short
    [InlineData("nouppercase1")] // no uppercase
    [InlineData("NoDigitsHere")] // no digit
    public void Weak_password_fails(string password)
    {
        var request = ValidRequest();
        request.Password = password;
        request.ConfirmPassword = password;
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public void Mismatched_confirm_password_fails()
    {
        var request = ValidRequest();
        request.ConfirmPassword = "SomethingElse1";
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.ConfirmPassword);
    }
}
