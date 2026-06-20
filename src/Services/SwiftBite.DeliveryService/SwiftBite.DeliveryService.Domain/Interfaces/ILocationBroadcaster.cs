using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Domain.Interfaces
{
    // DeliveryService.Domain/Interfaces/ILocationBroadcaster.cs
    public interface ILocationBroadcaster
    {
        Task BroadcastLocationAsync(
            Guid orderId,
            string customerId,
            double latitude,
            double longitude,
            string partnerName,
            string status,
            CancellationToken ct = default);
    }
}
