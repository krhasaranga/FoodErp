using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class InvoiceConfiguration : IEntityTypeConfiguration<Invoice>
{
    public void Configure(EntityTypeBuilder<Invoice> builder)
    {
        builder.ToTable("Invoices");
        builder.HasKey(i => i.Id);

        builder.Property(i => i.TenantId).IsRequired();
        builder.Property(i => i.InvoiceNumber).IsRequired().HasMaxLength(50);
        builder.Property(i => i.Status).IsRequired();
        builder.Property(i => i.InvoiceDate).IsRequired();
        builder.Property(i => i.DueDate).IsRequired();
        builder.Property(i => i.Notes).HasMaxLength(1000);
        builder.Property(i => i.TotalAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(i => i.PaidAmount).IsRequired().HasColumnType("decimal(18,4)");
        builder.Property(i => i.CreatedAt).IsRequired();

        // Computed properties — not stored in DB
        builder.Ignore(i => i.OutstandingAmount);
        builder.Ignore(i => i.IsOverdue);

        builder.HasIndex(i => new { i.TenantId, i.InvoiceNumber }).IsUnique();
        builder.HasIndex(i => new { i.TenantId, i.CustomerId });
        builder.HasIndex(i => new { i.TenantId, i.Status });
        builder.HasIndex(i => new { i.TenantId, i.DueDate });

        builder.HasOne(i => i.Customer)
               .WithMany()
               .HasForeignKey(i => i.CustomerId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(i => i.SalesOrder)
               .WithMany()
               .HasForeignKey(i => i.SalesOrderId)
               .IsRequired(false)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasMany(i => i.Lines)
               .WithOne(l => l.Invoice)
               .HasForeignKey(l => l.InvoiceId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(i => i.Payments)
               .WithOne(p => p.Invoice)
               .HasForeignKey(p => p.InvoiceId)
               .OnDelete(DeleteBehavior.Restrict); // prevent accidental cascade delete of payments
    }
}
