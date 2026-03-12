namespace SwiftBite.NotificationService.Application.Events;

// ✅ Consumed from: swiftbite.order.placed
public class OrderPlacedNotificationEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string RestaurantName { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public DateTime PlacedAt { get; set; }
}