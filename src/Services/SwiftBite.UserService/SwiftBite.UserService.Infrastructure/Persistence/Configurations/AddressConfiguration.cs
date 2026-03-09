using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.UserService.Domain.Entities;
namespace SwiftBite.UserService.Infrastructure.Persistence.Configurations;

public class AddressConfiguration : IEntityTypeConfiguration<Address>
{
    public void Configure(EntityTypeBuilder<Address> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Label)
            .IsRequired()
            .HasMaxLength(30);

        builder.Property(a => a.FullAddress)
            .IsRequired()
            .HasMaxLength(250);

        builder.Property(a => a.Street)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.City)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.State)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(a => a.PinCode)
            .IsRequired()
            .HasMaxLength(10);

        builder.Property(a => a.Landmark)
            .HasMaxLength(100);

        builder.Property(a => a.Type)
            .HasConversion<string>();

        builder.ToTable("Addresses");
    }
}