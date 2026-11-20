using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class RecipeLineConfiguration : IEntityTypeConfiguration<RecipeLine>
{
    public void Configure(EntityTypeBuilder<RecipeLine> builder)
    {
        builder.ToTable("RecipeLines");
        builder.HasKey(l => l.Id);

        builder.Property(l => l.TenantId).IsRequired();
        builder.Property(l => l.RecipeId).IsRequired();
        builder.Property(l => l.IngredientProductId).IsRequired();
        builder.Property(l => l.Quantity).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(l => l.Notes).HasMaxLength(500);
        builder.Property(l => l.CreatedAt).IsRequired();

        builder.HasOne(l => l.IngredientProduct)
               .WithMany()
               .HasForeignKey(l => l.IngredientProductId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(l => l.Unit)
               .WithMany()
               .HasForeignKey(l => l.UnitId)
               .OnDelete(DeleteBehavior.Restrict);

        // A product should appear only once per recipe
        builder.HasIndex(l => new { l.RecipeId, l.IngredientProductId }).IsUnique();
    }
}
