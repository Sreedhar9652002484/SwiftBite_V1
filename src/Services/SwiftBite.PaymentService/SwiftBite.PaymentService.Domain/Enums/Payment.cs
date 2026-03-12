using SwiftBite.PaymentService.Domain.Enums;

namespace SwiftBite.PaymentService.Domain.Entities;

public class Payment
{
    public Guid Id { get; private set; }

    // ── Order reference ───────────────────────────────────
    public Guid OrderId { get; private set; }
    public string CustomerId { get; private set; }
        = string.Empty;

    // ── Amount ────────────────────────────────────────────
    public decimal Amount { get; private set; }
    public string Currency { get; private set; }
        = "INR";

    // ── Razorpay details ──────────────────────────────────
    public string? RazorpayOrderId { get; private set; }
    public string? RazorpayPaymentId { get; private set; }
    public string? RazorpaySignature { get; private set; }

    // ── Status ────────────────────────────────────────────
    public PaymentStatus Status { get; private set; }
    public PaymentMethod Method { get; private set; }

    // ── Refund ────────────────────────────────────────────
    public string? RefundId { get; private set; }
    public decimal? RefundAmount { get; private set; }
    public string? FailureReason { get; private set; }

    // ── Timestamps ────────────────────────────────────────
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? PaidAt { get; private set; }
    public DateTime? RefundedAt { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid orderId, string customerId,
        decimal amount, PaymentMethod method)
    {
        return new Payment
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            Amount = amount,
            Method = method,
            Status = PaymentStatus.Pending,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    // ✅ Razorpay order created
    public void SetRazorpayOrderId(string razorpayOrderId)
    {
        RazorpayOrderId = razorpayOrderId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ Payment captured successfully
    public void MarkCaptured(
        string razorpayPaymentId,
        string razorpaySignature)
    {
        RazorpayPaymentId = razorpayPaymentId;
        RazorpaySignature = razorpaySignature;
        Status = PaymentStatus.Captured;
        PaidAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ Payment failed
    public void MarkFailed(string reason)
    {
        Status = PaymentStatus.Failed;
        FailureReason = reason;
        UpdatedAt = DateTime.UtcNow;
    }

    // ✅ Refund processed
    public void MarkRefunded(
        string refundId, decimal refundAmount)
    {
        RefundId = refundId;
        RefundAmount = refundAmount;
        Status = RefundAmount >= Amount
            ? PaymentStatus.Refunded
            : PaymentStatus.PartialRefund;
        RefundedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}