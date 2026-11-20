using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class InvoiceLineConfiguration : IEntityTypeConfiguration<InvoiceLine>
{
    public void Configure(EntityTypeBuilder<InvoiceLine> builder)
    {
        builder.ToTable("InvoiceLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.InvoiceId).IsRequired();
        builder.Property(l => l.Description).IsRequired().HasMaxLength(500);
        builder.Property(l => l.Quantity).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.DiscountPercent).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(l => l.CreatedAt).IsRequired();

        // LineTotal is computed in C#, not stored
        builder.Ignore(l => l.LineTotal);

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
