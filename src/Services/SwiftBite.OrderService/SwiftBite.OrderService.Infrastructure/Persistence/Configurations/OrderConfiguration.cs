using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.OrderService.Domain.Entities;

namespace SwiftBite.OrderService.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        // Basic properties
        builder.Property(o => o.CustomerId)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.CustomerPhone)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(o => o.RestaurantName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.DeliveryAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(o => o.DeliveryCity)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.DeliveryPinCode)
            .IsRequired()
            .HasMaxLength(10);

        // Decimal precision
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

        // Enum to string conversions
        builder.Property(o => o.Status)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentStatus)
            .HasConversion<string>()
            .HasMaxLength(30)
            .IsRequired();

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(o => o.PaymentTransactionId)
            .HasMaxLength(100);

        builder.Property(o => o.SpecialInstructions)
            .HasMaxLength(500);

        // ==================== CONCURRENCY TOKEN ====================
        // Only RowVersion should be used as concurrency token
        builder.Property(o => o.RowVersion)
            .IsRowVersion();                    // This is correct and sufficient

        // Make sure UpdatedAt is NOT a concurrency token
        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // ==================== DATE TIME CONFIGURATION (Important) ====================
        ConfigureDateTimeProperties(builder);

        // Relationships
        builder.HasMany(o => o.Items)
            .WithOne(i => i.Order)
            .HasForeignKey(i => i.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.StatusHistory)
            .WithOne(h => h.Order)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes
        builder.HasIndex(o => o.CustomerId);
        builder.HasIndex(o => o.RestaurantId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.PlacedAt);

        builder.ToTable("Orders");
    }

    // Helper method to handle DateTime properly (Prevents wrong date/time issues)
    private static void ConfigureDateTimeProperties(EntityTypeBuilder<Order> builder)
    {
        // Force all DateTimes to be stored and read as UTC
        var utcConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime, DateTime>(
            v => v.ToUniversalTime(),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        var utcNullableConverter = new Microsoft.EntityFrameworkCore.Storage.ValueConversion.ValueConverter<DateTime?, DateTime?>(
            v => v.HasValue ? v.Value.ToUniversalTime() : null,
            v => v.HasValue ? DateTime.SpecifyKind(v.Value, DateTimeKind.Utc) : null);

        builder.Property(o => o.PlacedAt)
            .HasConversion(utcConverter)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .HasConversion(utcConverter)
            .IsRequired();

        builder.Property(o => o.EstimatedDeliveryAt)
            .HasConversion(utcNullableConverter);

        builder.Property(o => o.DeliveredAt)
            .HasConversion(utcNullableConverter);
    }
}