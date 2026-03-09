using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Configurations;

public class MenuCategoryConfiguration
    : IEntityTypeConfiguration<MenuCategory>
{
    public void Configure(EntityTypeBuilder<MenuCategory> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.Name)
            .IsRequired().HasMaxLength(50);

        builder.Property(c => c.Description)
            .HasMaxLength(200);

        // ✅ One category → many items
        builder.HasMany(c => c.MenuItems)
            .WithOne(i => i.Category)
            .HasForeignKey(i => i.CategoryId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.ToTable("MenuCategories");
    }
}