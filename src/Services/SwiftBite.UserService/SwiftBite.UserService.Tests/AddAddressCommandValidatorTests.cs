using FluentValidation.TestHelper;
using SwiftBite.UserService.Application.Addresses.Commands.AddAddress;
using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Tests;

public class AddAddressCommandValidatorTests
{
    private readonly AddAddressCommandValidator _validator = new();

    private static AddAddressCommand ValidCommand() => new(
        AuthUserId: "auth-1",
        Label: "Home",
        FullAddress: "123 Main St, Springfield",
        Street: "Main St",
        City: "Springfield",
        State: "IL",
        PinCode: "560001",
        Latitude: 12.9716,
        Longitude: 77.5946,
        Type: AddressType.Home
    );

    [Fact]
    public void Valid_Command_Has_No_Errors()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_Label_Has_Error()
    {
        var command = ValidCommand() with { Label = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void Label_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { Label = new string('a', 31) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Label);
    }

    [Fact]
    public void Empty_FullAddress_Has_Error()
    {
        var command = ValidCommand() with { FullAddress = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullAddress);
    }

    [Fact]
    public void FullAddress_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { FullAddress = new string('a', 251) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.FullAddress);
    }

    [Fact]
    public void Empty_Street_Has_Error()
    {
        var command = ValidCommand() with { Street = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Fact]
    public void Street_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { Street = new string('a', 101) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Street);
    }

    [Fact]
    public void Empty_City_Has_Error()
    {
        var command = ValidCommand() with { City = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void City_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { City = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Empty_State_Has_Error()
    {
        var command = ValidCommand() with { State = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void State_Too_Long_Has_Error()
    {
        var command = ValidCommand() with { State = new string('a', 51) };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.State);
    }

    [Fact]
    public void Empty_PinCode_Has_Error()
    {
        var command = ValidCommand() with { PinCode = "" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PinCode);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    public void Invalid_PinCode_Format_Has_Error(string pinCode)
    {
        var command = ValidCommand() with { PinCode = pinCode };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.PinCode);
    }

    [Theory]
    [InlineData(-91)]
    [InlineData(91)]
    public void Latitude_Out_Of_Range_Has_Error(double latitude)
    {
        var command = ValidCommand() with { Latitude = latitude };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Latitude);
    }

    [Theory]
    [InlineData(-181)]
    [InlineData(181)]
    public void Longitude_Out_Of_Range_Has_Error(double longitude)
    {
        var command = ValidCommand() with { Longitude = longitude };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Longitude);
    }
}
