using SwiftBite.DeliveryService.Domain.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SwiftBite.DeliveryService.Domain.Interfaces
{
    public interface IDeliveryJobRepository
    {
        Task<DeliveryJob?> GetByIdAsync(Guid id, CancellationToken ct = default);

        Task<DeliveryJob?> GetByOrderIdAsync(Guid orderId, CancellationToken ct = default);

        Task<List<DeliveryJob>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct = default);
        Task<List<DeliveryJob>> GetActiveByPartnerIdAsync(Guid partnerId, CancellationToken ct = default);
        Task AddAsync(DeliveryJob job, CancellationToken ct = default);
        Task SaveChangesAsync(CancellationToken ct = default);
        Task<List<DeliveryJob>> GetAvailableJobsAsync(CancellationToken ct = default);

    }
}
