using MediatR;
using SwiftBite.PaymentService.Application.Events;
using SwiftBite.PaymentService.Application.Payments.DTOs;
using SwiftBite.PaymentService.Domain.Interfaces;

namespace SwiftBite.PaymentService.Application.Payments.Commands.VerifyPayment;

public class VerifyPaymentCommandHandler
    : IRequestHandler<VerifyPaymentCommand, PaymentDto>
{
    private readonly IPaymentRepository _repo;
    private readonly IRazorpayService _razorpay;
    private readonly IEventPublisher _publisher;

    public VerifyPaymentCommandHandler(
        IPaymentRepository repo,
        IRazorpayService razorpay,
        IEventPublisher publisher)
    {
        _repo = repo;
        _razorpay = razorpay;
        _publisher = publisher;
    }

    public async Task<PaymentDto> Handle(
        VerifyPaymentCommand cmd, CancellationToken ct)
    {
        // ✅ Find payment by Razorpay order ID
        var payment = await _repo
            .GetByRazorpayOrderIdAsync(
                cmd.RazorpayOrderId, ct)
            ?? throw new KeyNotFoundException(
                "Payment not found.");

        // 🔐 Verify Razorpay signature — CRITICAL security step!
        var isValid = _razorpay.VerifySignature(
            cmd.RazorpayOrderId,
            cmd.RazorpayPaymentId,
            cmd.RazorpaySignature);

        if (!isValid)
        {
            payment.MarkFailed("Invalid payment signature.");
            await _repo.UpdateAsync(payment, ct);
            await _repo.SaveChangesAsync(ct);

            // 🔥 Publish failure event
            await _publisher.PublishAsync(
                "swiftbite.payment.failed",
                new PaymentFailedEvent
                {
                    PaymentId = payment.Id,
                    OrderId = payment.OrderId,
                    CustomerId = payment.CustomerId,
                    Amount = payment.Amount,
                    FailureReason = "Invalid signature",
                    FailedAt = DateTime.UtcNow
                }, ct);

            throw new UnauthorizedAccessException(
                "Payment signature verification failed.");
        }

        // ✅ Mark as captured
        payment.MarkCaptured(
            cmd.RazorpayPaymentId,
            cmd.RazorpaySignature);

        await _repo.UpdateAsync(payment, ct);
        await _repo.SaveChangesAsync(ct);

        // 🔥 Publish success to Kafka!
        await _publisher.PublishAsync(
            "swiftbite.payment.success",
            new PaymentSuccessEvent
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                CustomerId = payment.CustomerId,
                Amount = payment.Amount,
                RazorpayPaymentId = cmd.RazorpayPaymentId,
                PaidAt = payment.PaidAt!.Value
            }, ct);

        return MapToDto(payment);
    }

    public static PaymentDto MapToDto(
        Domain.Entities.Payment p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            CustomerId = p.CustomerId,
            Amount = p.Amount,
            Currency = p.Currency,
            RazorpayOrderId = p.RazorpayOrderId,
            RazorpayPaymentId = p.RazorpayPaymentId,
            Status = p.Status,
            Method = p.Method,
            FailureReason = p.FailureReason,
            RefundAmount = p.RefundAmount,
            CreatedAt = p.CreatedAt,
            PaidAt = p.PaidAt,
            RefundedAt = p.RefundedAt
        };
}