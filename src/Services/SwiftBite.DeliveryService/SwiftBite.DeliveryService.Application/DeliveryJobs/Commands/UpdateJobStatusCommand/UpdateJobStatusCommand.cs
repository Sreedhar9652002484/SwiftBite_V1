using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;


// ── Update Job Status (PickedUp → Delivered) ──────────────────
public record UpdateJobStatusCommand(
    Guid JobId,
    string UserId,
    JobStatus NewStatus) : IRequest<DeliveryJobDto>;
