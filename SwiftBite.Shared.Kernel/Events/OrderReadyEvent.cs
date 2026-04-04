// SwiftBite.Shared.Kernel/Events/OrderReadyEvent.cs

namespace SwiftBite.Shared.Kernel.Events;
public class OrderReadyEvent
{
    public Guid OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty; // ✅ ADD
    public string OrderNumber { get; set; } = string.Empty;
        public Guid RestaurantId { get; set; }

    public string RestaurantName { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public string DeliveryAddress { get; set; } = string.Empty;
    public string DeliveryCity { get; set; } = string.Empty;
    public string DeliveryPinCode { get; set; } = string.Empty;
    public double DeliveryLatitude { get; set; }
    public double DeliveryLongitude { get; set; }
    public decimal DeliveryFee { get; set; }
    public DateTime ReadyAt { get; set; }
}