using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetPartnerProfile;

public record GetPartnerProfileQuery(string UserId) : IRequest<DeliveryPartnerDto>;

