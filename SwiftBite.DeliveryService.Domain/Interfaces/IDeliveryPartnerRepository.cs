using SwiftBite.DeliveryService.Domain.Domain.Entities;

namespace SwiftBite.DeliveryService.Domain.Domain.Interfaces;

public interface IDeliveryPartnerRepository
{
    Task<DeliveryPartner?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<DeliveryPartner?> GetByUserIdAsync(string userId, CancellationToken ct = default);
    Task<bool> ExistsByUserIdAsync(string userId, CancellationToken ct = default);
    Task AddAsync(DeliveryPartner partner, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}

