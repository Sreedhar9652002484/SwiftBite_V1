using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using SwiftBite.DeliveryService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetAvailableJobsQuery
{
    // Handler
    public class GetAvailableJobsQueryHandler
        : IRequestHandler<GetAvailableJobsQuery, List<DeliveryJobDto>>
    {
        private readonly IDeliveryJobRepository _jobRepo;

        public GetAvailableJobsQueryHandler(IDeliveryJobRepository jobRepo)
            => _jobRepo = jobRepo;

        public async Task<List<DeliveryJobDto>> Handle(
            GetAvailableJobsQuery query, CancellationToken ct)
        {
            // ✅ Return ALL jobs with no partner assigned yet
            var jobs = await _jobRepo.GetAvailableJobsAsync(ct);
            return jobs.Select(j => j.ToDto()).ToList();
        }
    }
}
