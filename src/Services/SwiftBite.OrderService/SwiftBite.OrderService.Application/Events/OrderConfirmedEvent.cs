namespace SwiftBite.OrderService.Application.Events;

// ✅ Published to: swiftbite.order.confirmed
// ✅ Consumed by: DeliveryService
public class OrderConfirmedEvent
{
    public Guid OrderId { get; set; }
    public Guid RestaurantId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int EstimatedPrepTimeMinutes { get; set; }
    public DateTime ConfirmedAt { get; set; }
}