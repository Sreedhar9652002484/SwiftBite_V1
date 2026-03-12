namespace SwiftBite.NotificationService.Application.Events;

// ✅ Consumed from: swiftbite.order.confirmed
public class OrderStatusChangedEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string NewStatus { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public int EstimatedMinutes { get; set; }
    public DateTime ChangedAt { get; set; }
}