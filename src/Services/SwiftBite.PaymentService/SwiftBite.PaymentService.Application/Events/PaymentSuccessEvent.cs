namespace SwiftBite.PaymentService.Application.Events;

// ✅ Published to: swiftbite.payment.success
// ✅ Consumed by: OrderService, NotificationService
public class PaymentSuccessEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string RazorpayPaymentId { get; set; } = string.Empty;
    public string PaymentMethod { get; set; } = string.Empty;
    public DateTime PaidAt { get; set; }
}