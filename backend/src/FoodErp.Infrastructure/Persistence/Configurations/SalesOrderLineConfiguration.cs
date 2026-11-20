using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class SalesOrderLineConfiguration : IEntityTypeConfiguration<SalesOrderLine>
{
    public void Configure(EntityTypeBuilder<SalesOrderLine> builder)
    {
        builder.ToTable("SalesOrderLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.SalesOrderId).IsRequired();
        builder.Property(l => l.ProductId).IsRequired();
        builder.Property(l => l.UnitId).IsRequired();
        builder.Property(l => l.Quantity).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.UnitPrice).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.DiscountPercent).IsRequired().HasColumnType("decimal(5,2)");
        builder.Property(l => l.CreatedAt).IsRequired();

        // LineTotal is computed in C#, not stored — no column mapping needed

        builder.HasOne(l => l.Product)
               .WithMany()
               .HasForeignKey(l => l.ProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Unit)
               .WithMany()
               .HasForeignKey(l => l.UnitId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
