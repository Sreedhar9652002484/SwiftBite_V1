using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetEarnings;

public record GetEarningsQuery(string UserId) : IRequest<EarningsDto>;

    