using MediatR;
using SwiftBite.PaymentService.Application.Payments.DTOs;

namespace SwiftBite.PaymentService.Application.Payments.Queries.GetCustomerPayments;

public record GetCustomerPaymentsQuery(string CustomerId)
    : IRequest<IEnumerable<PaymentDto>>;