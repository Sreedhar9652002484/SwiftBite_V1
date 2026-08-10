namespace SwiftBite.NotificationService.Application.Events;

// ✅ Consumed from: swiftbite.payment.success / failed
public class PaymentNotificationEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public bool IsSuccess { get; set; }
    public string? FailureReason { get; set; }
    public DateTime ProcessedAt { get; set; }
}