using FluentValidation.TestHelper;
using SwiftBite.AuthServer.Models;
using SwiftBite.AuthServer.Models.Validators;

namespace SwiftBite.AuthServer.Tests;

public class LoginRequestValidatorTests
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public void Valid_request_passes()
    {
        var request = new LoginRequest { Email = "jane.doe@example.com", Password = "anything" };
        var result = _validator.TestValidate(request);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_email_fails()
    {
        var request = new LoginRequest { Email = "", Password = "anything" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Invalid_email_format_fails()
    {
        var request = new LoginRequest { Email = "not-an-email", Password = "anything" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Empty_password_fails()
    {
        var request = new LoginRequest { Email = "jane.doe@example.com", Password = "" };
        var result = _validator.TestValidate(request);
        result.ShouldHaveValidationErrorFor(x => x.Password);
    }
}
