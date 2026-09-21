using ArchiSubscription.Core.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace ArchiSubscription.Infrastructure.Data.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Amount)
            .IsRequired()
            .HasPrecision(18, 2);

        builder.Property(p => p.PaymentDate)
            .IsRequired();

        builder.Property(p => p.Period)
            .IsRequired()
            .HasMaxLength(7); // "2026-05"

        builder.Property(p => p.Status)
            .IsRequired()
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(p => p.TransactionReference)
            .HasMaxLength(100);

        // Unique filtered index: only one successful payment per subscription per period (SQL Server)
        builder.HasIndex(p => new { p.SubscriptionId, p.Period })
            .HasFilter("[Status] = 'Successful'")
            .IsUnique();
    }
}

