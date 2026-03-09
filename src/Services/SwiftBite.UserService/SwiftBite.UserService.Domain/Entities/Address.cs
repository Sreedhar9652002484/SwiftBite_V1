using SwiftBite.UserService.Domain.Enums;

namespace SwiftBite.UserService.Domain.Entities;

public class Address
{
    public Guid Id { get; private set; }
    public Guid UserId { get; private set; }
    public string Label { get; private set; } = string.Empty; // "Home", "Office"
    public string FullAddress { get; private set; } = string.Empty;
    public string Street { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string State { get; private set; } = string.Empty;
    public string PinCode { get; private set; } = string.Empty;
    public string? Landmark { get; private set; }
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public bool IsDefault { get; private set; }
    public AddressType Type { get; private set; }
    public DateTime CreatedAt { get; private set; }

    // Navigation
    public User User { get; private set; } = null!;

    private Address() { }

    public static Address Create(
        Guid userId, string label, string fullAddress,
        string street, string city, string state,
        string pinCode, double latitude, double longitude,
        AddressType type, string? landmark = null)
    {
        return new Address
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Label = label,
            FullAddress = fullAddress,
            Street = street,
            City = city,
            State = state,
            PinCode = pinCode,
            Latitude = latitude,
            Longitude = longitude,
            Type = type,
            Landmark = landmark,
            IsDefault = false,
            CreatedAt = DateTime.UtcNow
        };
    }

    public void SetAsDefault() => IsDefault = true;
    public void UnsetDefault() => IsDefault = false;

    public void Update(string label, string fullAddress, string street,
        string city, string state, string pinCode,
        double latitude, double longitude, string? landmark)
    {
        Label = label;
        FullAddress = fullAddress;
        Street = street;
        City = city;
        State = state;
        PinCode = pinCode;
        Latitude = latitude;
        Longitude = longitude;
        Landmark = landmark;
    }
}