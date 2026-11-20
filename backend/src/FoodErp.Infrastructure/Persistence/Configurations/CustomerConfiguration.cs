using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class CustomerConfiguration : IEntityTypeConfiguration<Customer>
{
    public void Configure(EntityTypeBuilder<Customer> builder)
    {
        builder.ToTable("Customers");
        builder.HasKey(c => c.Id);

        builder.Property(c => c.TenantId).IsRequired();
        builder.Property(c => c.Code).IsRequired().HasMaxLength(50);
        builder.Property(c => c.Name).IsRequired().HasMaxLength(300);
        builder.Property(c => c.Type).IsRequired();
        builder.Property(c => c.ContactPerson).HasMaxLength(200);
        builder.Property(c => c.Phone).HasMaxLength(50);
        builder.Property(c => c.Email).HasMaxLength(200);
        builder.Property(c => c.Address).HasMaxLength(500);
        builder.Property(c => c.CreditLimit).HasColumnType("decimal(18,4)");
        builder.Property(c => c.IsActive).IsRequired();
        builder.Property(c => c.CreatedAt).IsRequired();

        // Customer codes unique per tenant
        builder.HasIndex(c => new { c.TenantId, c.Code }).IsUnique();
        builder.HasIndex(c => new { c.TenantId, c.Name });
    }
}
