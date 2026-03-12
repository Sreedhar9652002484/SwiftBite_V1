using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.NotificationService.Domain.Entities;

namespace SwiftBite.NotificationService.Infrastructure.Persistence.Configurations;

public class NotificationConfiguration
    : IEntityTypeConfiguration<Notification>
{
    public void Configure(
        EntityTypeBuilder<Notification> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.UserId)
            .IsRequired().HasMaxLength(100);

        builder.Property(n => n.Title)
            .IsRequired().HasMaxLength(200);

        builder.Property(n => n.Message)
            .IsRequired().HasMaxLength(1000);

        builder.Property(n => n.ReferenceId)
            .HasMaxLength(100);

        builder.Property(n => n.ImageUrl)
            .HasMaxLength(500);

        builder.Property(n => n.Type)
            .HasConversion<string>().HasMaxLength(50);

        builder.Property(n => n.Channel)
            .HasConversion<string>().HasMaxLength(30);

        // ✅ Indexes for fast queries
        builder.HasIndex(n => n.UserId);
        builder.HasIndex(n => n.IsRead);
        builder.HasIndex(n => n.CreatedAt);
        builder.HasIndex(n =>
            new { n.UserId, n.IsRead });

        builder.ToTable("Notifications");
    }
}