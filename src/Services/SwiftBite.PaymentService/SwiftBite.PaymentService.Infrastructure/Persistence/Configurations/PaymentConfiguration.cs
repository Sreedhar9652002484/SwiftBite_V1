using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using SwiftBite.PaymentService.Domain.Entities;

namespace SwiftBite.PaymentService.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration
    : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.CustomerId)
            .IsRequired().HasMaxLength(100);

        builder.Property(p => p.Currency)
            .IsRequired().HasMaxLength(10);

        builder.Property(p => p.Amount)
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.RefundAmount)
            .HasColumnType("decimal(10,2)");

        builder.Property(p => p.RazorpayOrderId)
            .HasMaxLength(100);

        builder.Property(p => p.RazorpayPaymentId)
            .HasMaxLength(100);

        builder.Property(p => p.RazorpaySignature)
            .HasMaxLength(500);

        builder.Property(p => p.RefundId)
            .HasMaxLength(100);

        builder.Property(p => p.FailureReason)
            .HasMaxLength(500);

        builder.Property(p => p.Status)
            .HasConversion<string>().HasMaxLength(30);

        builder.Property(p => p.Method)
            .HasConversion<string>().HasMaxLength(30);

        // ✅ Unique index — one payment per Razorpay order
        builder.HasIndex(p => p.RazorpayOrderId)
            .IsUnique();

        builder.HasIndex(p => p.OrderId);
        builder.HasIndex(p => p.CustomerId);
        builder.HasIndex(p => p.Status);

        builder.ToTable("Payments");
    }
}