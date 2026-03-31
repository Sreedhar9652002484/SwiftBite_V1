using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetActiveJob;

// ── Get all jobs for this partner ────────────────────────────
public record GetPartnerJobsQuery(string UserId) : IRequest<List<DeliveryJobDto>>;
