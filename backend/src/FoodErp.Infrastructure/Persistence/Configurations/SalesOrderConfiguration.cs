using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class SalesOrderConfiguration : IEntityTypeConfiguration<SalesOrder>
{
    public void Configure(EntityTypeBuilder<SalesOrder> builder)
    {
        builder.ToTable("SalesOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId).IsRequired();
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.OrderDate).IsRequired();
        builder.Property(o => o.DeliveryAddress).HasMaxLength(500);
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.CreatedAt).IsRequired();

        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.CustomerId });
        builder.HasIndex(o => new { o.TenantId, o.Status });

        builder.HasOne(o => o.Customer)
               .WithMany(c => c.SalesOrders)
               .HasForeignKey(o => o.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.Warehouse)
               .WithMany()
               .HasForeignKey(o => o.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lines)
               .WithOne(l => l.SalesOrder)
               .HasForeignKey(l => l.SalesOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
