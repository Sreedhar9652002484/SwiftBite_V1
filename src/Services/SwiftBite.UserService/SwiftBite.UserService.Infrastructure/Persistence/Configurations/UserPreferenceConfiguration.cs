using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.UserService.Domain.Entities;

namespace SwiftBite.UserService.Infrastructure.Persistence.Configurations;

public class UserPreferenceConfiguration
    : IEntityTypeConfiguration<UserPreference>
{
    public void Configure(EntityTypeBuilder<UserPreference> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.DietaryPreference)
            .HasConversion<string>();

        builder.Property(p => p.PreferredCuisines)
            .HasMaxLength(500)
            .HasDefaultValueSql("[]");

        builder.Property(p => p.AllergiesInfo)
            .HasMaxLength(250);

        builder.ToTable("UserPreferences");
    }
}