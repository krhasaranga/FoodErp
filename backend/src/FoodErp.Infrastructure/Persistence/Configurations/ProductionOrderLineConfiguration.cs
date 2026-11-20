using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class ProductionOrderLineConfiguration : IEntityTypeConfiguration<ProductionOrderLine>
{
    public void Configure(EntityTypeBuilder<ProductionOrderLine> builder)
    {
        builder.ToTable("ProductionOrderLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.ProductionOrderId).IsRequired();
        builder.Property(l => l.IngredientProductId).IsRequired();
        builder.Property(l => l.PlannedQuantity).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.ActualQuantity).HasColumnType("decimal(18,4)");
        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasOne(l => l.IngredientProduct)
               .WithMany()
               .HasForeignKey(l => l.IngredientProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Unit)
               .WithMany()
               .HasForeignKey(l => l.UnitId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
