using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;


namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Commands.AcceptJob;

// ── Accept Job ────────────────────────────────────────────────
public record AcceptJobCommand(
    Guid JobId,
    string UserId) : IRequest<DeliveryJobDto>;

