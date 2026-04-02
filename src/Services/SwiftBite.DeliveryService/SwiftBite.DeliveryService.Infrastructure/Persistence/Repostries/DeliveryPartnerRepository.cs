using Microsoft.EntityFrameworkCore;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;


namespace SwiftBite.DeliveryService.Infrastructure.Persistence.Repositories;

public class DeliveryPartnerRepository : IDeliveryPartnerRepository
{
    private readonly DeliveryDbContext _db;
    public DeliveryPartnerRepository(DeliveryDbContext db) => _db = db;

    public Task<DeliveryPartner?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.DeliveryPartners.FirstOrDefaultAsync(p => p.Id == id, ct);

    public Task<DeliveryPartner?> GetByUserIdAsync(string userId, CancellationToken ct = default)
        => _db.DeliveryPartners.FirstOrDefaultAsync(p => p.UserId == userId, ct);

    public Task<bool> ExistsByUserIdAsync(string userId, CancellationToken ct = default)
        => _db.DeliveryPartners.AnyAsync(p => p.UserId == userId, ct);

    public async Task AddAsync(DeliveryPartner partner, CancellationToken ct = default)
        => await _db.DeliveryPartners.AddAsync(partner, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}

