using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.RestaurantService.Domain.Entities;

namespace SwiftBite.RestaurantService.Infrastructure.Persistence.Configurations;

public class RestaurantHoursConfiguration
    : IEntityTypeConfiguration<RestaurantHours>
{
    public void Configure(EntityTypeBuilder<RestaurantHours> builder)
    {
        builder.HasKey(h => h.Id);

        builder.Property(h => h.DayOfWeek)
            .HasConversion<string>().HasMaxLength(10);

        builder.ToTable("RestaurantHours");
    }
}