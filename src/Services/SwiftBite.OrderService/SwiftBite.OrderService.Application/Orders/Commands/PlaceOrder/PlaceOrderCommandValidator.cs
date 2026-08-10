using FluentValidation;

namespace SwiftBite.OrderService.Application.Orders.Commands.PlaceOrder;

public class PlaceOrderCommandValidator
    : AbstractValidator<PlaceOrderCommand>
{
    public PlaceOrderCommandValidator()
    {
        RuleFor(x => x.CustomerId)
            .NotEmpty();
        RuleFor(x => x.RestaurantId)
            .NotEmpty();
        RuleFor(x => x.DeliveryAddress)
            .NotEmpty().MaximumLength(250);
        RuleFor(x => x.DeliveryCity)
            .NotEmpty();
        RuleFor(x => x.DeliveryPinCode)
            .NotEmpty().Matches(@"^\d{6}$");
        RuleFor(x => x.PaymentMethod)
            .NotEmpty();
        RuleFor(x => x.Items)
            .NotEmpty()
            .WithMessage("Order must have at least one item.");
        RuleForEach(x => x.Items).ChildRules(item =>
        {
            item.RuleFor(i => i.Quantity)
                .GreaterThan(0);
            item.RuleFor(i => i.UnitPrice)
                .GreaterThan(0);
        });
    }
}