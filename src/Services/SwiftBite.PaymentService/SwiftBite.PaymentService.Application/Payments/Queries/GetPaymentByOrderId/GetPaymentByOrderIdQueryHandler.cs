using MediatR;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Interfaces;

namespace SwiftBite.PaymentService.Application.Payments.Queries.GetPaymentByOrderId;

public class GetPaymentByOrderIdQueryHandler
    : IRequestHandler<GetPaymentByOrderIdQuery, PaymentDto>
{
    private readonly IPaymentRepository _repo;

    public GetPaymentByOrderIdQueryHandler(
        IPaymentRepository repo) => _repo = repo;

    public async Task<PaymentDto> Handle(
        GetPaymentByOrderIdQuery query, CancellationToken ct)
    {
        var payment = await _repo
            .GetByOrderIdAsync(query.OrderId, ct)
            ?? throw new KeyNotFoundException(
                $"Payment for order {query.OrderId} not found.");

        return VerifyPaymentCommandHandler.MapToDto(payment);
    }
}