using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.DeliveryService.Domain.Domain.Entities;

namespace SwiftBite.DeliveryService.Infrastructure.Persistence.Configurations;

public class DeliveryJobConfiguration
    : IEntityTypeConfiguration<DeliveryJob>
{
    public void Configure(EntityTypeBuilder<DeliveryJob> builder)
    {
        builder.HasKey(j => j.Id);

        builder.Property(j => j.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(j => j.CustomerName).IsRequired().HasMaxLength(200);
        builder.Property(j => j.CustomerPhone).IsRequired().HasMaxLength(20);
        builder.Property(j => j.RestaurantName).IsRequired().HasMaxLength(200);
        builder.Property(j => j.PickupAddress).IsRequired().HasMaxLength(500);
        builder.Property(j => j.DeliveryAddress).IsRequired().HasMaxLength(500);
        builder.Property(j => j.DeliveryCity).IsRequired().HasMaxLength(100);
        builder.Property(j => j.DeliveryFee).HasPrecision(18, 2);
        builder.Property(j => j.Status).HasConversion<int>();

        builder.HasIndex(j => j.OrderId);
        builder.HasIndex(j => j.PartnerId);
    }
}