using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Enums;

namespace SwiftBite.PaymentService.Application.Payments.Commands.InitiatePayment;

public record InitiatePaymentCommand(
    Guid OrderId,
    string CustomerId,
    string CustomerName,
    string CustomerEmail,
    string CustomerPhone,
    decimal Amount,
    PaymentMethod Method
) : IRequest<PaymentInitiatedDto>;