using FluentValidation.TestHelper;
using SwiftBite.RestaurantService.Application.Restaurants.Commands.CreateRestaurant;
using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Tests.Restaurants.Commands.CreateRestaurant;

public class CreateRestaurantCommandValidatorTests
{
    private readonly CreateRestaurantCommandValidator _validator = new();

    private static CreateRestaurantCommand ValidCommand() => new(
        OwnerId: "owner-1",
        Name: "The Good Kitchen",
        Description: "Home-style food made fresh daily",
        PhoneNumber: "+919876543210",
        Email: "contact@goodkitchen.com",
        Address: "123 MG Road",
        City: "Pune",
        PinCode: "411001",
        Latitude: 18.5204,
        Longitude: 73.8567,
        CuisineType: CuisineType.Indian,
        MinimumOrderAmount: 100m,
        AverageDeliveryTimeMinutes: 30
    );

    [Fact]
    public void Should_NotHaveErrors_When_CommandIsValid()
    {
        var result = _validator.TestValidate(ValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_HaveError_When_NameIsEmpty()
    {
        var command = ValidCommand() with { Name = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_NameExceedsMaxLength()
    {
        var command = ValidCommand() with { Name = new string('a', 101) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public void Should_HaveError_When_DescriptionIsEmpty()
    {
        var command = ValidCommand() with { Description = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_HaveError_When_DescriptionExceedsMaxLength()
    {
        var command = ValidCommand() with { Description = new string('a', 501) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Description);
    }

    [Fact]
    public void Should_HaveError_When_PhoneNumberIsEmpty()
    {
        var command = ValidCommand() with { PhoneNumber = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_HaveError_When_PhoneNumberDoesNotMatchPattern()
    {
        var command = ValidCommand() with { PhoneNumber = "12345" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PhoneNumber);
    }

    [Fact]
    public void Should_HaveError_When_EmailIsEmpty()
    {
        var command = ValidCommand() with { Email = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_EmailIsInvalid()
    {
        var command = ValidCommand() with { Email = "not-an-email" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Should_HaveError_When_AddressIsEmpty()
    {
        var command = ValidCommand() with { Address = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Should_HaveError_When_AddressExceedsMaxLength()
    {
        var command = ValidCommand() with { Address = new string('a', 251) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Address);
    }

    [Fact]
    public void Should_HaveError_When_CityIsEmpty()
    {
        var command = ValidCommand() with { City = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Should_HaveError_When_CityExceedsMaxLength()
    {
        var command = ValidCommand() with { City = new string('a', 51) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.City);
    }

    [Fact]
    public void Should_HaveError_When_PinCodeIsEmpty()
    {
        var command = ValidCommand() with { PinCode = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PinCode);
    }

    [Fact]
    public void Should_HaveError_When_PinCodeDoesNotMatchPattern()
    {
        var command = ValidCommand() with { PinCode = "12A456" };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PinCode);
    }

    [Fact]
    public void Should_HaveError_When_MinimumOrderAmountIsZero()
    {
        var command = ValidCommand() with { MinimumOrderAmount = 0m };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.MinimumOrderAmount);
    }

    [Fact]
    public void Should_HaveError_When_AverageDeliveryTimeIsBelowRange()
    {
        var command = ValidCommand() with { AverageDeliveryTimeMinutes = 9 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AverageDeliveryTimeMinutes);
    }

    [Fact]
    public void Should_HaveError_When_AverageDeliveryTimeIsAboveRange()
    {
        var command = ValidCommand() with { AverageDeliveryTimeMinutes = 121 };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.AverageDeliveryTimeMinutes);
    }
}
