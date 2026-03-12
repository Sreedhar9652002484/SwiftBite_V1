namespace SwiftBite.PaymentService.Application.Events;

// ✅ Published to: swiftbite.payment.failed
// ✅ Consumed by: OrderService, NotificationService
public class PaymentFailedEvent
{
    public Guid PaymentId { get; set; }
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string FailureReason { get; set; } = string.Empty;
    public DateTime FailedAt { get; set; }
}