using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using SwiftBite.NotificationService.Domain.Entities;
using SwiftBite.NotificationService.Infrastructure.Persistence.Converters;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace SwiftBite.NotificationService.Infrastructure.Persistence;

public class NotificationDbContext : DbContext
{
    public NotificationDbContext(
        DbContextOptions<NotificationDbContext> options)
        : base(options) { }

    public DbSet<Notification> Notifications
        => Set<Notification>();
    public DbSet<UserDevice> UserDevices
        => Set<UserDevice>();

    protected override void OnModelCreating(
        ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(
            typeof(NotificationDbContext).Assembly);
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