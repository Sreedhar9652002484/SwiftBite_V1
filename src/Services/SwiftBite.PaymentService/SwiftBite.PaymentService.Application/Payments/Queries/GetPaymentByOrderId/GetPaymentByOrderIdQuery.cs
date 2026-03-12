using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;

namespace SwiftBite.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;

public record GetPaymentByOrderIdQuery(Guid OrderId)
    : IRequest<PaymentDto>;