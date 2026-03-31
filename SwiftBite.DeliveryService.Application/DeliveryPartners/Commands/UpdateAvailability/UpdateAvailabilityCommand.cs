using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Commands.UpdateAvailability;

public record UpdateAvailabilityCommand(
    string UserId,
    bool IsAvailable) : IRequest<DeliveryPartnerDto>;

