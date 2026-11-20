using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class ProductionOrderConfiguration : IEntityTypeConfiguration<ProductionOrder>
{
    public void Configure(EntityTypeBuilder<ProductionOrder> builder)
    {
        builder.ToTable("ProductionOrders");
        builder.HasKey(o => o.Id);

        builder.Property(o => o.TenantId).IsRequired();
        builder.Property(o => o.OrderNumber).IsRequired().HasMaxLength(50);
        builder.Property(o => o.PlannedBatches).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(o => o.ActualBatches).HasColumnType("decimal(18,4)");
        builder.Property(o => o.Status).IsRequired();
        builder.Property(o => o.PlannedDate).IsRequired();
        builder.Property(o => o.Notes).HasMaxLength(1000);
        builder.Property(o => o.CreatedAt).IsRequired();

        // Order numbers are unique per tenant
        builder.HasIndex(o => new { o.TenantId, o.OrderNumber }).IsUnique();
        builder.HasIndex(o => new { o.TenantId, o.Status });

        builder.HasOne(o => o.Recipe)
               .WithMany()
               .HasForeignKey(o => o.RecipeId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.InputWarehouse)
               .WithMany()
               .HasForeignKey(o => o.InputWarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(o => o.OutputWarehouse)
               .WithMany()
               .HasForeignKey(o => o.OutputWarehouseId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(o => o.Lines)
               .WithOne(l => l.ProductionOrder)
               .HasForeignKey(l => l.ProductionOrderId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
