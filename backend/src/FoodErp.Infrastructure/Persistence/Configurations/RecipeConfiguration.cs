using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class RecipeConfiguration : IEntityTypeConfiguration<Recipe>
{
    public void Configure(EntityTypeBuilder<Recipe> builder)
    {
        builder.ToTable("Recipes");
        builder.HasKey(r => r.Id);

        builder.Property(r => r.TenantId).IsRequired();
        builder.Property(r => r.Name).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Description).HasMaxLength(1000);
        builder.Property(r => r.OutputQuantity).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(r => r.IsActive).IsRequired();
        builder.Property(r => r.CreatedAt).IsRequired();

        builder.HasOne(r => r.OutputProduct)
               .WithMany()
               .HasForeignKey(r => r.OutputProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.OutputUnit)
               .WithMany()
               .HasForeignKey(r => r.OutputUnitId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasMany(r => r.Lines)
               .WithOne(l => l.Recipe)
               .HasForeignKey(l => l.RecipeId)
               .OnDelete(DeleteBehavior.Cascade); // delete lines when recipe is deleted

        builder.HasIndex(r => new { r.TenantId, r.OutputProductId });
    }
}
