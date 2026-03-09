using SwiftBite.RestaurantService.Domain.Enums;

namespace SwiftBite.RestaurantService.Application.Restaurants.DTOs;

public class RestaurantDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public string City { get; set; } = string.Empty;
    public string PinCode { get; set; } = string.Empty;
    public double Latitude { get; set; }
    public double Longitude { get; set; }
    public string? LogoUrl { get; set; }
    public string? BannerUrl { get; set; }
    public CuisineType CuisineType { get; set; }
    public RestaurantStatus Status { get; set; }
    public double AverageRating { get; set; }
    public int TotalRatings { get; set; }
    public decimal MinimumOrderAmount { get; set; }
    public int AverageDeliveryTimeMinutes { get; set; }
    public bool IsOpen { get; set; }
    public DateTime CreatedAt { get; set; }
}