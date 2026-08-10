using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.RegisterPartner;

// ── Command ──────────────────────────────────────────────────
public record RegisterPartnerCommand(
    string UserId,
    string FirstName,
    string LastName,
    string Email,
    string PhoneNumber,
    VehicleType VehicleType,
    string VehicleNumber) : IRequest<DeliveryPartnerDto>;