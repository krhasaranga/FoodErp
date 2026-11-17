using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Code).IsRequired().HasMaxLength(50);
        builder.Property(p => p.Name).IsRequired().HasMaxLength(300);
        builder.Property(p => p.Description).HasMaxLength(1000);
        builder.Property(p => p.Type).IsRequired();
        builder.Property(p => p.TenantId).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        builder.Property(p => p.CostPrice)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(p => p.SellingPrice)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.Property(p => p.ReorderLevel)
            .IsRequired()
            .HasColumnType("decimal(18,4)");

        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Unit)
            .WithMany()
            .HasForeignKey(p => p.UnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Product codes are unique per tenant
        builder.HasIndex(p => new { p.TenantId, p.Code }).IsUnique();
    }
}