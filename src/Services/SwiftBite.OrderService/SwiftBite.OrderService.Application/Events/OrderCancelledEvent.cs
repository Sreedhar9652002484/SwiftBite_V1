namespace SwiftBite.OrderService.Application.Events;

// ✅ Published to: swiftbite.order.cancelled
// ✅ Consumed by: PaymentService, RestaurantService
public class OrderCancelledEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public Guid RestaurantId { get; set; }
    public decimal RefundAmount { get; set; }
    public string Reason { get; set; } = string.Empty;
    public DateTime CancelledAt { get; set; }
}