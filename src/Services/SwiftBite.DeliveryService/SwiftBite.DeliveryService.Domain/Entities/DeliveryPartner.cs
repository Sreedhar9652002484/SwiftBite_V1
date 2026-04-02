using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Domain.Domain.Entities;

public class DeliveryPartner
{
    public Guid Id { get; private set; }
    public string UserId { get; private set; } = default!;
    public string FirstName { get; private set; } = default!;
    public string LastName { get; private set; } = default!;
    public string Email { get; private set; } = default!;
    public string PhoneNumber { get; private set; } = default!;
    public VehicleType VehicleType { get; private set; }
    public string VehicleNumber { get; private set; } = default!;
    public bool IsAvailable { get; private set; }
    public double Rating { get; private set; }
    public int TotalDeliveries { get; private set; }
    public decimal TotalEarnings { get; private set; }
    public PartnerStatus Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private DeliveryPartner() { }

    public ICollection<DeliveryJob> Jobs { get; private set; } = new List<DeliveryJob>();

    public static DeliveryPartner Create(
        string userId,
        string firstName,
        string lastName,
        string email,
        string phoneNumber,
        VehicleType vehicleType,
        string vehicleNumber)
    {
        return new DeliveryPartner
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            PhoneNumber = phoneNumber,
            VehicleType = vehicleType,
            VehicleNumber = vehicleNumber,
            IsAvailable = false,
            Rating = 0,
            TotalDeliveries = 0,
            TotalEarnings = 0,
            Status = PartnerStatus.Active,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow,
        };
    }

    public void SetAvailability(bool isAvailable)
    {
        IsAvailable = isAvailable;
        UpdatedAt = DateTime.UtcNow;
    }

    public void CompleteDelivery(decimal earnings)
    {
        TotalDeliveries++;
        TotalEarnings += earnings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating)
    {
        Rating = newRating;
        UpdatedAt = DateTime.UtcNow;
    }

}


