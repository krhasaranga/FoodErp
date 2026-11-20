using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class PaymentConfiguration : IEntityTypeConfiguration<Payment>
{
    public void Configure(EntityTypeBuilder<Payment> builder)
    {
        builder.ToTable("Payments");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.InvoiceId).IsRequired();
        builder.Property(p => p.Amount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(p => p.Method).IsRequired();
        builder.Property(p => p.PaymentDate).IsRequired();
        builder.Property(p => p.Reference).HasMaxLength(100);
        builder.Property(p => p.Notes).HasMaxLength(500);
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.HasIndex(p => new { p.TenantId, p.InvoiceId });
        builder.HasIndex(p => new { p.TenantId, p.PaymentDate });
    }
}
