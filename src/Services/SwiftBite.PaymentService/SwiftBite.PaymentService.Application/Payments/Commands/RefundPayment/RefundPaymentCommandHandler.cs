using MediatR;
using SwiftBite.PaymentService.Application.Events;
using SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Interfaces;

namespace SwiftBite.PaymentService.Application.Payments.Commands.RefundPayment;

public class RefundPaymentCommandHandler
    : IRequestHandler<RefundPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _repo;
    private readonly IRazorpayService _razorpay;
    private readonly IEventPublisher _publisher;

    public RefundPaymentCommandHandler(
        IPaymentRepository repo,
        IRazorpayService razorpay,
        IEventPublisher publisher)
    {
        _repo = repo;
        _razorpay = razorpay;
        _publisher = publisher;
    }

    public async Task<PaymentDto> Handle(
        RefundPaymentCommand cmd, CancellationToken ct)
    {
        var payment = await _repo
            .GetByOrderIdAsync(cmd.OrderId, ct)
            ?? throw new KeyNotFoundException(
                "Payment not found.");

        if (payment.CustomerId != cmd.CustomerId)
            throw new UnauthorizedAccessException(
                "You don't own this payment.");

        if (payment.Status !=
            Domain.Enums.PaymentStatus.Captured)
            throw new InvalidOperationException(
                "Only captured payments can be refunded.");

        var refundAmt = cmd.RefundAmount
            ?? payment.Amount; // Full refund if null

        // ✅ Process refund via Razorpay
        var refundId = await _razorpay.ProcessRefundAsync(
            payment.RazorpayPaymentId!, refundAmt, ct);

        payment.MarkRefunded(refundId, refundAmt);

        await _repo.UpdateAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);

        return VerifyPaymentCommandHandler.MapToDto(payment);
    }
}