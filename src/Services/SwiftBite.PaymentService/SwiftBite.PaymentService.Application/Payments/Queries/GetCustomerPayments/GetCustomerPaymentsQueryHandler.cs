using MediatR;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Interfaces;

namespace SwiftBite.PaymentService.Application.Payments.Queries.GetCustomerPayments;

public class GetCustomerPaymentsQueryHandler
    : IRequestHandler<GetCustomerPaymentsQuery,
        IEnumerable<PaymentDto>>
{
    private readonly IPaymentRepository _repo;

    public GetCustomerPaymentsQueryHandler(
        IPaymentRepository repo) => _repo = repo;

    public async Task<IEnumerable<PaymentDto>> Handle(
        GetCustomerPaymentsQuery query, CancellationToken ct)
    {
        var payments = await _repo
            .GetByCustomerIdAsync(query.CustomerId, ct);

        return payments
            .OrderByDescending(p => p.CreatedAt)
            .Select(VerifyPaymentCommandHandler.MapToDto);
    }
}