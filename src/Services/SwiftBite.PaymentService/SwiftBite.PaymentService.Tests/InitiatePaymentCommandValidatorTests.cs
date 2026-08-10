using FluentValidation.TestHelper;
using SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;
using SwiftBite.PaymentService.Domain.Enums;

namespace SwiftBite.PaymentService.Tests;

public class InitiatePaymentCommandValidatorTests
{
    private readonly InitiatePaymentCommandValidator _validator = new();

    private static InitiatePaymentCommand ValidCommand() => new(
        OrderId: Guid.NewGuid(),
        CustomerId: "customer-1",
        CustomerName: "Jane Doe",
        CustomerEmail: "jane.doe@example.com",
        CustomerPhone: "+919876543210",
        Amount: 250.50m,
        Method: PaymentMethod.UPI);

    [Fact]
    public void Valid_command_passes()
    {
        var result = _validator.TestValidate(ValidCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Empty_order_id_fails()
    {
        var command = ValidCommand() with { OrderId = Guid.Empty };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.OrderId);
    }

    [Fact]
    public void Invalid_email_fails()
    {
        var command = ValidCommand() with { CustomerEmail = "not-an-email" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerEmail);
    }

    [Fact]
    public void Invalid_phone_fails()
    {
        var command = ValidCommand() with { CustomerPhone = "123" };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.CustomerPhone);
    }

    [Fact]
    public void Zero_amount_fails()
    {
        var command = ValidCommand() with { Amount = 0 };
        var result = _validator.TestValidate(command);
        result.ShouldHaveValidationErrorFor(x => x.Amount);
    }
}
