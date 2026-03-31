using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Application.DTOs;

public record DeliveryPartnerDto(
    Guid Id,
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    string VehicleType,
    string VehicleNumber,
    bool IsAvailable,
    double Rating,
    int TotalDeliveries,
    decimal TotalEarnings,
    string Status,
    DateTime CreatedAt);

public record DeliveryJobDto(
    Guid Id,
    Guid OrderId,
    string OrderNumber,
    string CustomerName,
    string CustomerPhone,
    string RestaurantName,
    string PickupAddress,
    string DeliveryAddress,
    string DeliveryCity,
    decimal DeliveryFee,
    string Status,
    DateTime AssignedAt,
    DateTime? AcceptedAt,
    DateTime? PickedUpAt,
    DateTime? DeliveredAt);

public record EarningsDto(
    decimal TotalEarnings,
    int TotalDeliveries,
    double Rating,
    List<DeliveryJobDto> RecentJobs);