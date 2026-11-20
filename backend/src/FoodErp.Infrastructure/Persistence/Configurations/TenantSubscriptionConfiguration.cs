using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class TenantSubscriptionConfiguration : IEntityTypeConfiguration<TenantSubscription>
{
    public void Configure(EntityTypeBuilder<TenantSubscription> builder)
    {
        builder.ToTable("TenantSubscriptions");
        builder.HasKey(s => s.Id);

        builder.Property(s => s.TenantId).IsRequired();
        builder.Property(s => s.PlanId).IsRequired();
        builder.Property(s => s.Status).IsRequired();
        builder.Property(s => s.StartDate).IsRequired();
        builder.Property(s => s.Notes).HasMaxLength(500);
        builder.Property(s => s.CreatedAt).IsRequired();

        // One subscription per tenant
        builder.HasIndex(s => s.TenantId).IsUnique();
        builder.HasIndex(s => s.Status);

        builder.HasOne(s => s.Tenant)
               .WithMany()
               .HasForeignKey(s => s.TenantId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Plan)
               .WithMany()
               .HasForeignKey(s => s.PlanId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
