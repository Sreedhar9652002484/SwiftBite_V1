namespace SwiftBite.AuthServer.Models;

public class PartnerApplication
{
    public Guid Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string RequestedRole { get; set; } = string.Empty; // RestaurantAdmin or DeliveryPartner
    public string Status { get; set; } = "Pending"; // Pending, Approved, Rejected

    // Restaurant applicants
    public string? BusinessName { get; set; }
    public string? City { get; set; }

    // Delivery partner applicants
    public string? VehicleType { get; set; }
    public string? LicenseNumber { get; set; }

    public string Phone { get; set; } = string.Empty;
    public string? Note { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; set; }
    public string? ReviewedByUserId { get; set; }
    public string? ReviewNote { get; set; }
}
