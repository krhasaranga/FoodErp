using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class WarehouseConfiguration : IEntityTypeConfiguration<Warehouse>
{
    public void Configure(EntityTypeBuilder<Warehouse> builder)
    {
        builder.ToTable("Warehouses");
        builder.HasKey(w => w.Id);

        builder.Property(w => w.Name).IsRequired().HasMaxLength(200);
        builder.Property(w => w.Location).HasMaxLength(500);
        builder.Property(w => w.TenantId).IsRequired();
        builder.Property(w => w.IsActive).IsRequired();
        builder.Property(w => w.CreatedAt).IsRequired();

        // Warehouse names must be unique within a tenant
        builder.HasIndex(w => new { w.TenantId, w.Name }).IsUnique();
    }
}
