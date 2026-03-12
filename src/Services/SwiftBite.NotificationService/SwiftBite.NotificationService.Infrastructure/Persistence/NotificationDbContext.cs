using Microsoft.EntityFrameworkCore;
using SwiftBite.NotificationService.Domain.Entities;
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
}