namespace SwiftBite.NotificationService.Domain.Enums;

public enum NotificationType
{
    OrderPlaced = 1,
    OrderConfirmed = 2,
    OrderPreparing = 3,
    OrderReady = 4,
    OrderPickedUp = 5,
    OrderOutForDelivery = 6,
    OrderDelivered = 7,
    OrderCancelled = 8,
    PaymentSuccess = 9,
    PaymentFailed = 10,
    PromotionalOffer = 11,
    GeneralAlert = 12
}