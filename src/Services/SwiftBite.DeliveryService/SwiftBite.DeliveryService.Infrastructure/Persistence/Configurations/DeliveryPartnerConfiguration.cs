using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.DeliveryService.Domain.Domain.Entities;

namespace SwiftBite.DeliveryService.Infrastructure.Persistence.Configurations;

public class DeliveryPartnerConfiguration
    : IEntityTypeConfiguration<DeliveryPartner>
{
    public void Configure(EntityTypeBuilder<DeliveryPartner> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.UserId).IsRequired().HasMaxLength(450);
        builder.Property(p => p.FirstName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.LastName).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Email).IsRequired().HasMaxLength(256);
        builder.Property(p => p.PhoneNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.VehicleNumber).IsRequired().HasMaxLength(20);
        builder.Property(p => p.TotalEarnings).HasPrecision(18, 2);
        builder.Property(p => p.VehicleType).HasConversion<int>();
        builder.Property(p => p.Status).HasConversion<int>();
        builder.HasIndex(p => p.UserId).IsUnique();

        // ✅ FIX: optional relationship — partner can be null on job
        builder.HasMany(p => p.Jobs)
               .WithOne(j => j.Partner)
               .HasForeignKey(j => j.PartnerId)
               .IsRequired(false)                    // ← nullable FK
               .OnDelete(DeleteBehavior.SetNull);    // ← don't cascade delete jobs
    }
}