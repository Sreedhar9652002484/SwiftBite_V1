using MediatR;
using SwiftBite.DeliveryService.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Application.DeliveryJobs.Queries.GetAvailableJobsQuery
{
    public record GetAvailableJobsQuery : IRequest<List<DeliveryJobDto>>;

}
