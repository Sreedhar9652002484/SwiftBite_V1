using MediatR;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.DeliveryService.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Application.Location.Commands.UpdateLocation
{
    // Handler
    public class UpdateLocationCommandHandler
        : IRequestHandler<UpdateLocationCommand>
    {
        private readonly IDeliveryPartnerRepository _partnerRepo;
        private readonly IDeliveryJobRepository _jobRepo;
        private readonly ILocationBroadcaster _broadcaster;

        public UpdateLocationCommandHandler(
            IDeliveryPartnerRepository partnerRepo,
            IDeliveryJobRepository jobRepo,
            ILocationBroadcaster broadcaster)
        {
            _partnerRepo = partnerRepo;
            _jobRepo = jobRepo;
            _broadcaster = broadcaster;
        }

        public async Task Handle(
            UpdateLocationCommand cmd, CancellationToken ct)
        {
            var partner = await _partnerRepo.GetByUserIdAsync(cmd.UserId, ct)
                ?? throw new KeyNotFoundException("Partner not found.");

            // ✅ Update partner location in DB
            partner.UpdateLocation(cmd.Latitude, cmd.Longitude);
            await _partnerRepo.SaveChangesAsync(ct);

            // ✅ Find active job for this partner
            var activeJob = await _jobRepo
                .GetActiveByPartnerIdAsync(partner.Id, ct);

            var currentJob = activeJob.FirstOrDefault(
                j => j.Status == JobStatus.Accepted
                  || j.Status == JobStatus.PickedUp);

            if (currentJob is null) return;

            // ✅ Broadcast location to customer tracking this order
            await _broadcaster.BroadcastLocationAsync(
                currentJob.OrderId,
                currentJob.CustomerId,
                cmd.Latitude,
                cmd.Longitude,
                partner.FirstName + " " + partner.LastName,
                currentJob.Status.ToString(),
                ct);
        }
    }
}
