using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.DeliveryService.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetActiveJobsQuery;

public class GetActiveJobsQueryHandler
    : IRequestHandler<GetActiveJobsQuery, List<DeliveryJobDto>>
{
    private readonly IDeliveryPartnerRepository _partnerRepo;
    private readonly IDeliveryJobRepository _jobRepo;

    public GetActiveJobsQueryHandler(
        IDeliveryPartnerRepository partnerRepo,
        IDeliveryJobRepository jobRepo)
    {
        _partnerRepo = partnerRepo;
        _jobRepo = jobRepo;
    }

    public async Task<List<DeliveryJobDto>> Handle(
        GetActiveJobsQuery query, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByUserIdAsync(query.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        var jobs = await _jobRepo.GetActiveByPartnerIdAsync(partner.Id, ct);

        return jobs.Select(j => j.ToDto()).ToList();
    }
}