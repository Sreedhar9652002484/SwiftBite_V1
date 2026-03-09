using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Configurations;

public class RestaurantConfiguration : IEntityTypeConfiguration<Restaurant>
{
    public void Configure(EntityTypeBuilder<Restaurant> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.Name)
            .IsRequired().HasMaxLength(100);

        builder.Property(r => r.Description)
            .IsRequired().HasMaxLength(500);

        builder.Property(r => r.PhoneNumber)
            .IsRequired().HasMaxLength(20);

        builder.Property(r => r.Email)
            .IsRequired().HasMaxLength(100);

        builder.Property(r => r.Address)
            .IsRequired().HasMaxLength(250);

        builder.Property(r => r.City)
            .IsRequired().HasMaxLength(50);

        builder.Property(r => r.PinCode)
            .IsRequired().HasMaxLength(10);

        builder.Property(r => r.LogoUrl)
            .HasMaxLength(500);

        builder.Property(r => r.BannerUrl)
            .HasMaxLength(500);

        builder.Property(r => r.CuisineType)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(r => r.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(r => r.MinimumOrderAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(r => r.AverageRating)
            .HasColumnType("float");

        // ✅ One restaurant → many categories
        builder.HasMany(r => r.MenuCategories)
            .WithOne(c => c.Restaurant)
            .HasForeignKey(c => c.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        // ✅ One restaurant → many hours
        builder.HasMany(r => r.Hours)
            .WithOne(h => h.Restaurant)
            .HasForeignKey(h => h.RestaurantId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(r => r.City);
        builder.HasIndex(r => r.OwnerId);

        builder.ToTable("Restaurants");
    }
}