using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.OrderService.Domain.Entities;

namespace SwiftBite.OrderService.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration
    : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired().HasMaxLength(100);

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500);

        builder.Property(i => i.UnitPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.TotalPrice)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.Customization)
            .HasMaxLength(500);

        builder.ToTable("OrderItems");
    }
}