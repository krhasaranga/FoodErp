using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class StockBalanceConfiguration : IEntityTypeConfiguration<StockBalance>
{
    public void Configure(EntityTypeBuilder<StockBalance> builder)
    {
        builder.ToTable("StockBalances");
        builder.HasKey(b => b.Id);

        builder.Property(b => b.TenantId).IsRequired();
        builder.Property(b => b.WarehouseId).IsRequired();
        builder.Property(b => b.ProductId).IsRequired();
        builder.Property(b => b.Quantity)
               .IsRequired()
               .HasColumnType("decimal(18,4)");
        builder.Property(b => b.CreatedAt).IsRequired();

        // One balance row per product per warehouse per tenant
        builder.HasIndex(b => new { b.TenantId, b.WarehouseId, b.ProductId }).IsUnique();

        builder.HasOne(b => b.Warehouse)
               .WithMany(w => w.StockBalances)
               .HasForeignKey(b => b.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(b => b.Product)
               .WithMany()
               .HasForeignKey(b => b.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
