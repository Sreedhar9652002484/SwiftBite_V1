using Microsoft.EntityFrameworkCore;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SwiftBite.DeliveryService.Infrastructure.Persistence;

public class DeliveryDbContext : DbContext
{
    public DeliveryDbContext(DbContextOptions<DeliveryDbContext> options)
        : base(options) { }

    public DbSet<DeliveryPartner> DeliveryPartners => Set<DeliveryPartner>();
    public DbSet<DeliveryJob> DeliveryJobs => Set<DeliveryJob>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DeliveryDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}