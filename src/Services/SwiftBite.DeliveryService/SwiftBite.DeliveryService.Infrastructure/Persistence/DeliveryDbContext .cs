using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SwiftBite.DeliveryService.Domain.Domain.Entities;
using SwiftBite.DeliveryService.Infrastructure.Persistence.Converters;
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

    protected override void ConfigureConventions(
        ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);

        // Globally ensure every DateTime/DateTime? property round-trips as UTC,
        // since SQL Server's datetime2 does not persist DateTimeKind.
        configurationBuilder.Properties<DateTime>()
            .HaveConversion<UtcDateTimeConverter>();

        configurationBuilder.Properties<DateTime?>()
            .HaveConversion<NullableUtcDateTimeConverter>();
    }
}