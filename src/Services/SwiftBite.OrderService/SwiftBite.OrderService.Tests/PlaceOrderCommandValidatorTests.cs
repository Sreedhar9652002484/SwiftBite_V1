using FluentValidation.TestHelper;
using SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;
using SwiftBite.OrderService.Application.Orders.DTOs;

namespace SwiftBite.OrderService.Tests;

public class PlaceOrderCommandValidatorTests
{
    private readonly PlaceOrderCommandValidator _validator = new();

    private static PlaceOrderCommand CreateValidCommand() => new(
        CustomerId: "customer-1",
        CustomerName: "John Doe",
        CustomerPhone: "9876543210",
        RestaurantId: Guid.NewGuid(),
        RestaurantName: "Tasty Bites",
        DeliveryAddress: "123 Main Street",
        DeliveryCity: "Mumbai",
        DeliveryPinCode: "400001",
        DeliveryLatitude: 19.0760,
        DeliveryLongitude: 72.8777,
        PaymentMethod: "CARD",
        SpecialInstructions: null,
        Items: new List<OrderItemRequest>
        {
            new(MenuItemId: Guid.NewGuid(), Name: "Burger", Quantity: 2, UnitPrice: 150m)
        });

    [Fact]
    public void Should_Not_Have_Errors_When_Command_Is_Valid()
    {
        var result = _validator.TestValidate(CreateValidCommand());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Should_Have_Error_When_CustomerId_Is_Empty()
    {
        var command = CreateValidCommand() with { CustomerId = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.CustomerId);
    }

    [Fact]
    public void Should_Have_Error_When_RestaurantId_Is_Empty()
    {
        var command = CreateValidCommand() with { RestaurantId = Guid.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.RestaurantId);
    }

    [Fact]
    public void Should_Have_Error_When_DeliveryAddress_Is_Empty()
    {
        var command = CreateValidCommand() with { DeliveryAddress = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    [Fact]
    public void Should_Have_Error_When_DeliveryAddress_Exceeds_MaxLength()
    {
        var command = CreateValidCommand() with { DeliveryAddress = new string('a', 251) };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeliveryAddress);
    }

    [Fact]
    public void Should_Have_Error_When_DeliveryCity_Is_Empty()
    {
        var command = CreateValidCommand() with { DeliveryCity = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeliveryCity);
    }

    [Fact]
    public void Should_Have_Error_When_DeliveryPinCode_Is_Empty()
    {
        var command = CreateValidCommand() with { DeliveryPinCode = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeliveryPinCode);
    }

    [Theory]
    [InlineData("12345")]
    [InlineData("1234567")]
    [InlineData("abcdef")]
    [InlineData("400 001")]
    public void Should_Have_Error_When_DeliveryPinCode_Is_Malformed(string pinCode)
    {
        var command = CreateValidCommand() with { DeliveryPinCode = pinCode };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.DeliveryPinCode);
    }

    [Fact]
    public void Should_Have_Error_When_PaymentMethod_Is_Empty()
    {
        var command = CreateValidCommand() with { PaymentMethod = string.Empty };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.PaymentMethod);
    }

    [Fact]
    public void Should_Have_Error_When_Items_Is_Empty()
    {
        var command = CreateValidCommand() with { Items = new List<OrderItemRequest>() };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor(x => x.Items);
    }

    [Fact]
    public void Should_Have_Error_When_Item_Quantity_Is_Not_Positive()
    {
        var command = CreateValidCommand() with
        {
            Items = new List<OrderItemRequest>
            {
                new(MenuItemId: Guid.NewGuid(), Name: "Burger", Quantity: 0, UnitPrice: 150m)
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].Quantity");
    }

    [Fact]
    public void Should_Have_Error_When_Item_UnitPrice_Is_Not_Positive()
    {
        var command = CreateValidCommand() with
        {
            Items = new List<OrderItemRequest>
            {
                new(MenuItemId: Guid.NewGuid(), Name: "Burger", Quantity: 1, UnitPrice: 0m)
            }
        };

        var result = _validator.TestValidate(command);

        result.ShouldHaveValidationErrorFor("Items[0].UnitPrice");
    }
}
