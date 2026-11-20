using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class StockMovementConfiguration : IEntityTypeConfiguration<StockMovement>
{
    public void Configure(EntityTypeBuilder<StockMovement> builder)
    {
        builder.ToTable("StockMovements");
        builder.HasKey(m => m.Id);

        builder.Property(m => m.TenantId).IsRequired();
        builder.Property(m => m.WarehouseId).IsRequired();
        builder.Property(m => m.ProductId).IsRequired();
        builder.Property(m => m.Type).IsRequired();

        builder.Property(m => m.Quantity)
               .IsRequired()
               .HasColumnType("decimal(18,4)");

        builder.Property(m => m.BalanceAfter)
               .IsRequired()
               .HasColumnType("decimal(18,4)");

        builder.Property(m => m.Reference).HasMaxLength(100);
        builder.Property(m => m.Notes).HasMaxLength(500);
        builder.Property(m => m.BatchNumber).HasMaxLength(100);
        builder.Property(m => m.MovedAt).IsRequired();
        builder.Property(m => m.CreatedAt).IsRequired();

        // Useful query patterns
        builder.HasIndex(m => new { m.TenantId, m.WarehouseId, m.MovedAt });
        builder.HasIndex(m => new { m.TenantId, m.ProductId, m.MovedAt });
        builder.HasIndex(m => m.TransferPairId);  // find both legs of a transfer

        builder.HasOne(m => m.Warehouse)
               .WithMany(w => w.StockMovements)
               .HasForeignKey(m => m.WarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Product)
               .WithMany()
               .HasForeignKey(m => m.ProductId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
