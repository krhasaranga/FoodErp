using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class UnitConversionConfiguration : IEntityTypeConfiguration<UnitConversion>
{
    public void Configure(EntityTypeBuilder<UnitConversion> builder)
    {
        builder.ToTable("UnitConversions");
        builder.HasKey(u => u.Id);

        builder.Property(u => u.Factor)
            .IsRequired()
            .HasColumnType("decimal(18,6)");

        builder.Property(u => u.TenantId).IsRequired();
        builder.Property(u => u.CreatedAt).IsRequired();

        builder.HasOne(u => u.FromUnit)
            .WithMany()
            .HasForeignKey(u => u.FromUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(u => u.ToUnit)
            .WithMany()
            .HasForeignKey(u => u.ToUnitId)
            .OnDelete(DeleteBehavior.Restrict);

        // Prevent duplicate conversion pairs per tenant
        builder.HasIndex(u => new { u.TenantId, u.FromUnitId, u.ToUnitId }).IsUnique();
    }
}