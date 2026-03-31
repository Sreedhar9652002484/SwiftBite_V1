using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetPartnerProfile;


public class GetPartnerProfileQueryHandler
    : IRequestHandler<GetPartnerProfileQuery, DeliveryPartnerDto>
{
    private readonly IDeliveryPartnerRepository _repo;

    public GetPartnerProfileQueryHandler(IDeliveryPartnerRepository repo)
        => _repo = repo;

    public async Task<DeliveryPartnerDto> Handle(
        GetPartnerProfileQuery query, CancellationToken ct)
    {
        var partner = await _repo.GetByUserIdAsync(query.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        return partner.ToDto();
    }
}