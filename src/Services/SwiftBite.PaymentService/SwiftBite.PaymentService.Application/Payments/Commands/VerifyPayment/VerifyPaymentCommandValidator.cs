using FluentValidation;

namespace SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;

public class VerifyPaymentCommandValidator
    : AbstractValidator<VerifyPaymentCommand>
{
    public VerifyPaymentCommandValidator()
    {
        RuleFor(x => x.RazorpayOrderId).NotEmpty();
        RuleFor(x => x.RazorpayPaymentId).NotEmpty();
        RuleFor(x => x.RazorpaySignature).NotEmpty();
    }
}