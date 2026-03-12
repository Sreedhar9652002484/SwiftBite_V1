namespace SwiftBite.PaymentService.Domain.Interfaces;

public interface IRazorpayService
{
    // ✅ Create Razorpay order
    Task<RazorpayOrderResult> CreateOrderAsync(
        decimal amount, string currency,
        string receipt, CancellationToken ct = default);

    // ✅ Verify payment signature
    bool VerifySignature(
        string razorpayOrderId,
        string razorpayPaymentId,
        string razorpaySignature);

    // ✅ Process refund
    Task<string> ProcessRefundAsync(
        string razorpayPaymentId,
        decimal amount,
        CancellationToken ct = default);
}

public record RazorpayOrderResult(
    string RazorpayOrderId,
    decimal Amount,
    string Currency,
    string Status);