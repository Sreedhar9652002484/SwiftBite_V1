using SwiftBite.OrderService.Domain.Enums;

namespace SwiftBite.OrderService.Domain.Entities;

public class OrderStatusHistory
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public OrderStatus Status { get; private set; }
    public string Note { get; private set; } = string.Empty;
    public DateTime Timestamp { get; private set; }

    // Navigation
    public Order Order { get; private set; } = null!;

    private OrderStatusHistory() { }

    public static OrderStatusHistory Create(
        Guid orderId, OrderStatus status, string note)
    {
        return new OrderStatusHistory
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            Status = status,
            Note = note,
            Timestamp = DateTime.UtcNow
        };
    }
}