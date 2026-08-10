using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.DeliveryService.Domain.Interfaces;
using SwiftBite.Shared.Kernel.Events;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;


public class UpdateJobStatusCommandHandler
    : IRequestHandler<UpdateJobStatusCommand, DeliveryJobDto>
{
    private readonly IDeliveryJobRepository _jobRepo;
    private readonly IDeliveryPartnerRepository _partnerRepo;
    private readonly IEventPublisher _publisher;


    public UpdateJobStatusCommandHandler(
        IDeliveryJobRepository jobRepo,
        IDeliveryPartnerRepository partnerRepo, IEventPublisher publisher)
    {
        _jobRepo = jobRepo;
        _partnerRepo = partnerRepo;
        _publisher = publisher;

    }


    public async Task<DeliveryJobDto> Handle(
        UpdateJobStatusCommand cmd, CancellationToken ct)
    {
        var partner = await _partnerRepo.GetByUserIdAsync(cmd.UserId, ct)
            ?? throw new KeyNotFoundException("Partner not found.");

        var job = await _jobRepo.GetByIdAsync(cmd.JobId, ct)
            ?? throw new KeyNotFoundException("Job not found.");

        if (job.PartnerId != partner.Id)
            throw new UnauthorizedAccessException("This job is not assigned to you.");

        switch (cmd.NewStatus)
        {
            case JobStatus.PickedUp:
                if (job.Status != JobStatus.Accepted)
                    throw new InvalidOperationException(
                        "Must accept job before marking picked up.");

                job.MarkPickedUp();

                await _publisher.PublishAsync(
                    "swiftbite.delivery.pickedup",
                    new DeliveryJobPickedUpEvent
                    {
                        OrderId = job.OrderId,
                        JobId = job.Id,
                        PartnerId = partner.Id,
                        CustomerId = job.CustomerId,  // ✅ added below
                        PickedUpAt = DateTime.UtcNow
                    }, ct);
                break;

            case JobStatus.Delivered:
                if (job.Status != JobStatus.PickedUp)
                    throw new InvalidOperationException(
                        "Must pick up order before marking delivered.");

                job.MarkDelivered();
                partner.CompleteDelivery(job.DeliveryFee);
                await _partnerRepo.SaveChangesAsync(ct);

                await _publisher.PublishAsync(
                    "swiftbite.delivery.delivered",
                    new DeliveryJobDeliveredEvent
                    {
                        OrderId = job.OrderId,
                        JobId = job.Id,
                        PartnerId = partner.Id,
                        CustomerId = job.CustomerId,  // ✅ added below
                        DeliveredAt = DateTime.UtcNow
                    }, ct);
                break;

            case JobStatus.Rejected:
                if (job.Status != JobStatus.Assigned)
                    throw new InvalidOperationException(
                        "Can only reject assigned jobs.");
                job.Reject();
                break;

            default:
                throw new InvalidOperationException(
                    $"Cannot set status to {cmd.NewStatus}.");
        }

        await _jobRepo.SaveChangesAsync(ct);
        return job.ToDto();
    }
}