using MediatR;

namespace SwiftBite.OrderService.Application.Orders.Commands.CancelOrder;

public record CancelOrderCommand(
    Guid OrderId,
    string CustomerId,
    string Reason
) : IRequest<bool>;