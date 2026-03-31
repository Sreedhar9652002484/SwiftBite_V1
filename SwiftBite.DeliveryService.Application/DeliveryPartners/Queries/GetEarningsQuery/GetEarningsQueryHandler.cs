using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryPartners.Queries.GetEarnings;
public class GetEarningsQueryHandler
    : IRequestHandler<GetEarningsQuery, EarningsDto>
{
    private readonly IDeliveryPartnerRepository _partnerRepo;
    private readonly IDeliveryJobRepository _jobRepo;

    public GetEarningsQueryHandler(
        IDeliveryPartnerRepository partnerRepo,
        IDeliveryJobRepository jobRepo)
    {
        _partnerRepo = partnerRepo;
        _jobRepo = jobRepo;
    }

    public async Task<EarningsDto> Handle(
        GetEarningsQuery query, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByUserIdAsync(query.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        var jobs = await _jobRepo.GetByPartnerIdAsync(partner.Id, ct);

        var recentJobs = jobs
            .OrderByDescending(j => j.AssignedAt)
            .Take(20)
            .Select(j => j.ToDto())
            .ToList();

        return new EarningsDto(
            partner.TotalEarnings,
            partner.TotalDeliveries,
            partner.Rating,
            recentJobs);
    }
}