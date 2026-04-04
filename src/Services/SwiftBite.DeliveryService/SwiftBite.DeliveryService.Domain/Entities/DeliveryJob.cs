using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Domain.Domain.Entities;

public class DeliveryJob
{
    public Guid Id { get; private set; }
    public Guid OrderId { get; private set; }
    public Guid? PartnerId { get; private set; }  // ← must be nullable
    public string CustomerId { get; private set; } = default!; // ✅ ADD

    public string OrderNumber { get; private set; } = default!;
    public string CustomerName { get; private set; } = default!;
    public string CustomerPhone { get; private set; } = default!;
    public string RestaurantName { get; private set; } = default!;
    public string PickupAddress { get; private set; } = default!;
    public string DeliveryAddress { get; private set; } = default!;
    public string DeliveryCity { get; private set; } = default!;
    public decimal DeliveryFee { get; private set; }
    public JobStatus Status { get; private set; }
    public DateTime AssignedAt { get; private set; }
    public DateTime? AcceptedAt { get; private set; }
    public DateTime? PickedUpAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    // Navigation
    public DeliveryPartner? Partner { get; private set; } // ← nullable navigation


    private DeliveryJob() { }

    public static DeliveryJob Create(
        Guid orderId,
        string customerId,
        string orderNumber,
        string customerName,
        string customerPhone,
        string restaurantName,
        string pickupAddress,
        string deliveryAddress,
        string deliveryCity,
        decimal deliveryFee,
        Guid? partnerId = null)  // ← optional, default null
    {
        return new DeliveryJob
        {
            Id = Guid.NewGuid(),
            OrderId = orderId,
            CustomerId = customerId,
            PartnerId = partnerId,        // ✅ no .Value!
            OrderNumber = orderNumber,
            CustomerName = customerName,
            CustomerPhone = customerPhone,
            RestaurantName = restaurantName,
            PickupAddress = pickupAddress,
            DeliveryAddress = deliveryAddress,
            DeliveryCity = deliveryCity,
            DeliveryFee = deliveryFee,
            Status = JobStatus.Assigned,
            AssignedAt = DateTime.UtcNow,
        };
    }

    //public void Accept()
    //{
    //    Status = JobStatus.Accepted;
    //    AcceptedAt = DateTime.UtcNow;
    //}
    public void AssignPartner(Guid partnerId)
    {
        PartnerId = partnerId;
        Status = JobStatus.Accepted;
        AcceptedAt = DateTime.UtcNow;
    }


    public void MarkPickedUp()
    {
        Status = JobStatus.PickedUp;
        PickedUpAt = DateTime.UtcNow;
    }

    public void MarkDelivered()
    {
        Status = JobStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
    }

    public void Reject()
    {
        Status = JobStatus.Rejected;
    }

    public bool CanAdvance() =>
        Status == JobStatus.Accepted || Status == JobStatus.PickedUp;
}