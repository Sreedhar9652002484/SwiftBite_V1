using FluentValidation;

namespace SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;

public class InitiatePaymentCommandValidator
    : AbstractValidator<InitiatePaymentCommand>
{
    public InitiatePaymentCommandValidator()
    {
        RuleFor(x => x.OrderId).NotEmpty();
        RuleFor(x => x.CustomerId).NotEmpty();
        RuleFor(x => x.CustomerName).NotEmpty();
        RuleFor(x => x.CustomerEmail)
            .NotEmpty().EmailAddress();
        RuleFor(x => x.CustomerPhone)
            .NotEmpty().Matches(@"^\+?[1-9]\d{9,14}$");
        RuleFor(x => x.Amount).GreaterThan(0);
    }
}