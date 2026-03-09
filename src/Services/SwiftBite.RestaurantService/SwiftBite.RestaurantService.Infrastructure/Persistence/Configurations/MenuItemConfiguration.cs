using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Configurations;

public class MenuItemConfiguration : IEntityTypeConfiguration<MenuItem>
{
    public void Configure(EntityTypeBuilder<MenuItem> builder)
    {
        builder.HasKey(i => i.Id);

        builder.Property(i => i.Name)
            .IsRequired().HasMaxLength(100);

        builder.Property(i => i.Description)
            .IsRequired().HasMaxLength(500);

        builder.Property(i => i.Price)
            .HasColumnType("decimal(10,2)");

        builder.Property(i => i.ImageUrl)
            .HasMaxLength(500);

        builder.Property(i => i.Status)
            .HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(i => i.RestaurantId);
        builder.HasIndex(i => i.CategoryId);

        builder.ToTable("MenuItems");
    }
}