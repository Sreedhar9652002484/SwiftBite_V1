using Microsoft.EntityFrameworkCore;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using SwiftBite.DeliveryService.Domain.Domain.Interfaces;
using SwiftBite.DeliveryService.Domain.Enums;
using SwiftBite.DeliveryService.Domain.Interfaces;

namespace SwiftBite.DeliveryService.Infrastructure.Persistence.Repositories;

public class DeliveryJobRepository : IDeliveryJobRepository
{
    private readonly DeliveryDbContext _db;
    public DeliveryJobRepository(DeliveryDbContext db) => _db = db;

    public Task<DeliveryJob?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => _db.DeliveryJobs
              .Include(j => j.Partner)
              .FirstOrDefaultAsync(j => j.Id == id, ct);

    public Task<List<DeliveryJob>> GetByPartnerIdAsync(Guid partnerId, CancellationToken ct = default)
        => _db.DeliveryJobs
              .Where(j => j.PartnerId == partnerId)
              .OrderByDescending(j => j.AssignedAt)
              .ToListAsync(ct);

    public Task<List<DeliveryJob>> GetActiveByPartnerIdAsync(Guid partnerId, CancellationToken ct = default)
        => _db.DeliveryJobs
              .Where(j => j.PartnerId == partnerId &&
                     (j.Status == JobStatus.Assigned ||
                      j.Status == JobStatus.Accepted ||
                      j.Status == JobStatus.PickedUp))
              .OrderByDescending(j => j.AssignedAt)
              .ToListAsync(ct);

    public async Task AddAsync(DeliveryJob job, CancellationToken ct = default)
        => await _db.DeliveryJobs.AddAsync(job, ct);

    public Task SaveChangesAsync(CancellationToken ct = default)
        => _db.SaveChangesAsync(ct);
}