using FluentValidation.TestHelper;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;

namespace SwiftBite.PaymentService.Tests;

public class VerifyPaymentCommandValidatorTests
{
    private readonly VerifyPaymentCommandValidator _validator = new();

    [Fact]
    public void Valid_command_passes()
    {
        var command = new VerifyPaymentCommand("order_123", "pay_123", "sig_123");
        var result = _validator.TestValidate(command);
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Theory]
    [InlineData("", "pay_123", "sig_123")]
    [InlineData("order_123", "", "sig_123")]
    [InlineData("order_123", "pay_123", "")]
    public void Missing_required_field_fails(string orderId, string paymentId, string signature)
    {
        var command = new VerifyPaymentCommand(orderId, paymentId, signature);
        var result = _validator.TestValidate(command);
        Assert.False(result.IsValid);
    }
}
