using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Entities;

namespace SwiftBite.DeliveryService.Application;

public static class MappingExtensions
{
    public static DeliveryPartnerDto ToDto(this DeliveryPartner p) => new(
        p.Id,
        p.UserId,
        p.FirstName,
        p.LastName,
        p.Email,
        p.PhoneNumber,
        p.VehicleType.ToString(),
        p.VehicleNumber,
        p.IsAvailable,
        p.Rating,
        p.TotalDeliveries,
        p.TotalEarnings,
        p.Status.ToString(),
        p.CreatedAt);

    public static DeliveryJobDto ToDto(this DeliveryJob j) => new(
        j.Id,
        j.OrderId,
        j.OrderNumber,
        j.CustomerName,
        j.CustomerPhone,
        j.RestaurantName,
        j.PickupAddress,
        j.DeliveryAddress,
        j.DeliveryCity,
        j.DeliveryFee,
        j.Status.ToString(),
        j.AssignedAt,
        j.AcceptedAt,
        j.PickedUpAt,
        j.DeliveredAt);
}