using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Domain.Entities;

public class Restaurant
{
    public Guid Id { get; private set; }
    public string OwnerId { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Description { get; private set; } = string.Empty;
    public string PhoneNumber { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string City { get; private set; } = string.Empty;
    public string PinCode { get; private set; } = string.Empty;
    public double Latitude { get; private set; }
    public double Longitude { get; private set; }
    public string? LogoUrl { get; private set; }
    public string? BannerUrl { get; private set; }
    public CuisineType CuisineType { get; private set; }
    public RestaurantStatus Status { get; private set; }
    public double AverageRating { get; private set; }
    public int TotalRatings { get; private set; }
    public decimal MinimumOrderAmount { get; private set; }
    public int AverageDeliveryTimeMinutes { get; private set; }
    public bool IsOpen { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    // Navigation
    public ICollection<MenuCategory> MenuCategories { get; private set; }
        = new List<MenuCategory>();
    public ICollection<RestaurantHours> Hours { get; private set; }
        = new List<RestaurantHours>();

    private Restaurant() { }

    public static Restaurant Create(
        string ownerId, string name, string description,
        string phoneNumber, string email, string address,
        string city, string pinCode, double latitude,
        double longitude, CuisineType cuisineType,
        decimal minimumOrderAmount, int avgDeliveryTime)
    {
        return new Restaurant
        {
            Id = Guid.NewGuid(),
            OwnerId = ownerId,
            Name = name,
            Description = description,
            PhoneNumber = phoneNumber,
            Email = email,
            Address = address,
            City = city,
            PinCode = pinCode,
            Latitude = latitude,
            Longitude = longitude,
            CuisineType = cuisineType,
            MinimumOrderAmount = minimumOrderAmount,
            AverageDeliveryTimeMinutes = avgDeliveryTime,
            Status = RestaurantStatus.PendingApproval,
            AverageRating = 0,
            TotalRatings = 0,
            IsOpen = false,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(string name, string description,
        string phoneNumber, string address, string city,
        string pinCode, decimal minimumOrderAmount,
        int avgDeliveryTime, CuisineType cuisineType)
    {
        Name = name;
        Description = description;
        PhoneNumber = phoneNumber;
        Address = address;
        City = city;
        PinCode = pinCode;
        MinimumOrderAmount = minimumOrderAmount;
        AverageDeliveryTimeMinutes = avgDeliveryTime;
        CuisineType = cuisineType;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Approve()
    {
        Status = RestaurantStatus.Active;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ToggleOpen()
    {
        IsOpen = !IsOpen;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateRating(double newRating)
    {
        TotalRatings++;
        AverageRating = ((AverageRating * (TotalRatings - 1))
            + newRating) / TotalRatings;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdateImages(string? logoUrl, string? bannerUrl)
    {
        LogoUrl = logoUrl;
        BannerUrl = bannerUrl;
        UpdatedAt = DateTime.UtcNow;
    }
}