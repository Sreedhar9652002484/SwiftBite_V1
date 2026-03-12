using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;

namespace SwiftBite.PaymentService.Application.Payments.Commands.RefundPayment;

public record RefundPaymentCommand(
    Guid OrderId,
    string CustomerId,
    decimal? RefundAmount = null  // null = full refund
) : IRequest<PaymentDto>;
