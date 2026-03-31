using SwiftBite.OrderService.Domain.Entities;
using SwiftBite.OrderService.Domain.Enums;

namespace SwiftBite.OrderService.Domain.Entities;

public class Order
{
    public Guid Id { get; private set; }

    // ── Who ordered ───────────────────────────────────────
    public string CustomerId { get; private set; } = string.Empty;
    public string CustomerName { get; private set; } = string.Empty;
    public string CustomerPhone { get; private set; } = string.Empty;

    // ── From which restaurant ─────────────────────────────
    public Guid RestaurantId { get; private set; }
    public string RestaurantName { get; private set; } = string.Empty;

    // ── Delivery details ──────────────────────────────────
    public string DeliveryAddress { get; private set; } = string.Empty;
    public string DeliveryCity { get; private set; } = string.Empty;
    public string DeliveryPinCode { get; private set; } = string.Empty;
    public double DeliveryLatitude { get; private set; }
    public double DeliveryLongitude { get; private set; }

    // ── Pricing ───────────────────────────────────────────
    public decimal SubTotal { get; private set; }
    public decimal DeliveryFee { get; private set; }
    public decimal Taxes { get; private set; }
    public decimal Discount { get; private set; }
    public decimal TotalAmount { get; private set; }

    // ── Status ────────────────────────────────────────────
    public OrderStatus Status { get; private set; }
    public PaymentStatus PaymentStatus { get; private set; }
    public string PaymentMethod { get; private set; } = string.Empty;
    public string? PaymentTransactionId { get; private set; }

    // ── Special instructions ──────────────────────────────
    public string? SpecialInstructions { get; private set; }

    // ── Timestamps ────────────────────────────────────────
    public DateTime PlacedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? EstimatedDeliveryAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }
    public byte[]? RowVersion { get; set; }

    // ── Navigation ────────────────────────────────────────
    public ICollection<OrderItem> Items { get; private set; }
        = new List<OrderItem>();
    public ICollection<OrderStatusHistory> StatusHistory { get; private set; }
        = new List<OrderStatusHistory>();

    private Order() { }

    // ── Factory method ────────────────────────────────────
    public static Order Create(
        string customerId, string customerName,
        string customerPhone, Guid restaurantId,
        string restaurantName, string deliveryAddress,
        string deliveryCity, string deliveryPinCode,
        double deliveryLatitude, double deliveryLongitude,
        string paymentMethod, string? specialInstructions,
        List<OrderItem> items, decimal deliveryFee = 30m)
    {
        var subTotal = items.Sum(i => i.TotalPrice);
        var taxes = Math.Round(subTotal * 0.05m, 2); // 5% GST
        var total = subTotal + deliveryFee + taxes;

        var order = new Order
        {
            Id = Guid.NewGuid(),
            CustomerId = customerId,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            RestaurantId = restaurantId,
            RestaurantName = restaurantName,
            DeliveryAddress = deliveryAddress,
            DeliveryCity = deliveryCity,
            DeliveryPinCode = deliveryPinCode,
            DeliveryLatitude = deliveryLatitude,
            DeliveryLongitude = deliveryLongitude,
            PaymentMethod = paymentMethod,
            SpecialInstructions = specialInstructions,
            SubTotal = subTotal,
            DeliveryFee = deliveryFee,
            Taxes = taxes,
            Discount = 0,
            TotalAmount = total,
            Status = OrderStatus.Pending,
            PaymentStatus = PaymentStatus.Pending,
            PlacedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
            EstimatedDeliveryAt = DateTime.UtcNow.AddMinutes(45),
            Items = items
        };

        // ✅ Record initial status history
        order.StatusHistory.Add(OrderStatusHistory.Create(
            order.Id, OrderStatus.Pending,
            "Order placed successfully."));

        return order;
    }

    // ── Status transitions (like real Swiggy!) ────────────
    public void Confirm()
        => Transition(OrderStatus.Confirmed,
            "Order confirmed by restaurant.");

    public void StartPreparing()
        => Transition(OrderStatus.Preparing,
            "Restaurant started preparing your food.");

    public void MarkReady()
        => Transition(OrderStatus.Ready,
            "Food is ready for pickup.");

    public void MarkPickedUp()
        => Transition(OrderStatus.PickedUp,
            "Delivery partner picked up your order.");

    public void MarkOutForDelivery()
        => Transition(OrderStatus.OutForDelivery,
            "Your order is out for delivery.");

    public void MarkDelivered()
    {
        Transition(OrderStatus.Delivered,
            "Order delivered successfully!");
        DeliveredAt = DateTime.UtcNow;
        PaymentStatus = PaymentStatus.Paid;
    }

    public void Cancel(string reason)
    {
        if (Status >= OrderStatus.PickedUp)
            throw new InvalidOperationException(
                "Cannot cancel order after pickup.");

        Transition(OrderStatus.Cancelled,
            $"Order cancelled: {reason}");
    }

    public void ApplyDiscount(decimal discount)
    {
        Discount = discount;
        TotalAmount = SubTotal + DeliveryFee + Taxes - Discount;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetPaymentTransactionId(string transactionId)
    {
        PaymentTransactionId = transactionId;
        UpdatedAt = DateTime.UtcNow;
    }

    // ── Private helper ────────────────────────────────────
    private void Transition(OrderStatus newStatus, string note)
    {
        Status = newStatus;
        UpdatedAt = DateTime.UtcNow;
        StatusHistory.Add(
            OrderStatusHistory.Create(Id, newStatus, note));
    }
}