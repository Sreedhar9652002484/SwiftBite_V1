using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.DeliveryService.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;



public class AcceptJobCommandHandler
    : IRequestHandler<AcceptJobCommand, DeliveryJobDto>
{
    private readonly IDeliveryJobRepository _jobRepo;
    private readonly IDeliveryPartnerRepository _partnerRepo;

    public AcceptJobCommandHandler(
        IDeliveryJobRepository jobRepo,
        IDeliveryPartnerRepository partnerRepo)
    {
        _jobRepo = jobRepo;
        _partnerRepo = partnerRepo;
    }

    public async Task<DeliveryJobDto> Handle(
        AcceptJobCommand cmd, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByUserIdAsync(cmd.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        var job = await _jobRepo.GetByIdAsync(cmd.JobId, ct)
            ?? throw new KeyNotFoundException("Job not found.");

        if (job.PartnerId != partner.Id)
            throw new UnauthorizedAccessException("This job is not assigned to you.");

        if (job.Status != JobStatus.Assigned)
            throw new InvalidOperationException("Job cannot be accepted in its current state.");

        job.Accept();
        await _jobRepo.SaveChangesAsync(ct);

        return job.ToDto();
    }
}
