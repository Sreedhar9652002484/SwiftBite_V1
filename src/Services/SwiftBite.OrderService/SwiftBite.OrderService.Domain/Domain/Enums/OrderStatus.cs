namespace SwiftBite.OrderService.Domain.Enums;

public enum OrderStatus
{
    Pending = 1,  // Order placed, waiting for restaurant
    Confirmed = 2,  // Restaurant accepted
    Preparing = 3,  // Kitchen is preparing
    Ready = 4,  // Food ready for pickup
    PickedUp = 5,  // Delivery partner picked up
    OutForDelivery = 6,  // On the way
    Delivered = 7,  // Successfully delivered
    Cancelled = 8,  // Cancelled
    Refunded = 9   // Refund processed
}