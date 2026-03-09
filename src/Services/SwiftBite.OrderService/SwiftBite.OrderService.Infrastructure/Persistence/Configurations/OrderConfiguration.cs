using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.OrderService.Domain.Entities;

namespace SwiftBite.OrderService.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.CustomerId)
            .IsRequired().HasMaxLength(100);

        builder.Property(o => o.CustomerName)
            .IsRequired().HasMaxLength(100);

        builder.Property(o => o.CustomerPhone)
            .IsRequired().HasMaxLength(20);

        builder.Property(o => o.RestaurantName)
            .IsRequired().HasMaxLength(100);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired().HasMaxLength(250);

        builder.Property(o => o.DeliveryCity)
            .IsRequired().HasMaxLength(50);

        builder.Property(o => o.DeliveryPinCode)
            .IsRequired().HasMaxLength(10);

        builder.Property(o => o.SubTotal)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.DeliveryFee)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.Taxes)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.Discount)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.TotalAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(o => o.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(o => o.PaymentMethod)
            .IsRequired().HasMaxLength(50);

        builder.Property(o => o.PaymentTransactionId)
            .HasMaxLength(100);

        builder.Property(o => o.SpecialInstructions)
            .HasMaxLength(500);

        // ✅ One Order → many Items
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ One Order → many Status History
        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PlacedAt);

        builder.ToTable("Orders");
    }
}