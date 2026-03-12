using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.NotificationService.Domain.Entities;

namespace SwiftBite.NotificationService.Infrastructure.Persistence.Configurations;

public class UserDeviceConfiguration
    : IEntityTypeConfiguration<UserDevice>
{
    public void Configure(
        EntityTypeBuilder<UserDevice> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.UserId)
            .IsRequired().HasMaxLength(100);

        builder.Property(d => d.DeviceToken)
            .IsRequired().HasMaxLength(500);

        builder.Property(d => d.DeviceType)
            .IsRequired().HasMaxLength(20);

        // ✅ Unique token per device
        builder.HasIndex(d => d.DeviceToken)
            .IsUnique();

        builder.HasIndex(d => d.UserId);

        builder.ToTable("UserDevices");
    }
}