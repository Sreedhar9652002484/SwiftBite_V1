using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.OrderService.Domain.Entities;

namespace SwiftBite.OrderService.Infrastructure.Persistence.Configurations;

public class OrderStatusHistoryConfiguration
    : IEntityTypeConfiguration<OrderStatusHistory>
{
    public void Configure(
        EntityTypeBuilder<OrderStatusHistory> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(h => h.Note)
            .IsRequired().HasMaxLength(500);

        builder.HasIndex(h => h.OrderId);

        builder.ToTable("OrderStatusHistory");
    }
}