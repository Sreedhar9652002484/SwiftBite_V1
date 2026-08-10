using SwiftBite.DeliveryService.Domain.Interfaces;
using SwiftBite.Shared.Kernel.Events;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Infrastructure.Persistence.Repostries
{
    // DeliveryService.Infrastructure/Messaging/KafkaLocationBroadcaster.cs
    public class KafkaLocationBroadcaster : ILocationBroadcaster
    {
        private readonly IEventPublisher _publisher;

        public KafkaLocationBroadcaster(IEventPublisher publisher)
            => _publisher = publisher;

        public async Task BroadcastLocationAsync(
            Guid orderId,
            string customerId,
            double latitude,
            double longitude,
            string partnerName,
            string status,
            CancellationToken ct = default)
        {
            await _publisher.PublishAsync(
                "swiftbite.location.updated",
                new LocationUpdatedEvent
                {
                    OrderId = orderId,
                    CustomerId = customerId,
                    Latitude = latitude,
                    Longitude = longitude,
                    PartnerName = partnerName,
                    Status = status,
                    UpdatedAt = DateTime.UtcNow
                }, ct);
        }
    }
}
