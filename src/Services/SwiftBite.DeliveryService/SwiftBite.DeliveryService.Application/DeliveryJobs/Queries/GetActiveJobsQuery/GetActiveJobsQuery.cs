using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetActiveJobsQuery;



// ── Get active jobs (Assigned / Accepted / PickedUp) ─────────
public record GetActiveJobsQuery(string UserId) : IRequest<List<DeliveryJobDto>>;

