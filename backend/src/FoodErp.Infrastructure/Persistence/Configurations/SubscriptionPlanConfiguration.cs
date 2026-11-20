using FoodErp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class SubscriptionPlanConfiguration : IEntityTypeConfiguration<SubscriptionPlan>
{
    public void Configure(EntityTypeBuilder<SubscriptionPlan> builder)
    {
        builder.ToTable("SubscriptionPlans");
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name).IsRequired().HasMaxLength(100);
        builder.Property(p => p.Description).HasMaxLength(500);
        builder.Property(p => p.PricePerMonth).IsRequired().HasColumnType("decimal(18,2)");
        builder.Property(p => p.MaxUsers).IsRequired();
        builder.Property(p => p.MaxProducts).IsRequired();
        builder.Property(p => p.MaxWarehouses).IsRequired();
        builder.Property(p => p.MaxMonthlyOrders).IsRequired();
        builder.Property(p => p.IsActive).IsRequired();
        builder.Property(p => p.CreatedAt).IsRequired();

        // Seed the standard plans
        builder.HasData(
            new
            {
                Id               = Guid.Parse("aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa"),
                Name             = "Starter",
                Description      = "Perfect for small food businesses just getting started.",
                PricePerMonth    = 29.00m,
                MaxUsers         = 3,
                MaxProducts      = 100,
                MaxWarehouses    = 1,
                MaxMonthlyOrders = 200,
                IsActive         = true,
                CreatedAt        = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt        = (DateTime?)null
            },
            new
            {
                Id               = Guid.Parse("bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb"),
                Name             = "Professional",
                Description      = "For growing food manufacturers and distributors.",
                PricePerMonth    = 79.00m,
                MaxUsers         = 10,
                MaxProducts      = 500,
                MaxWarehouses    = 5,
                MaxMonthlyOrders = 1000,
                IsActive         = true,
                CreatedAt        = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt        = (DateTime?)null
            },
            new
            {
                Id               = Guid.Parse("cccccccc-cccc-cccc-cccc-cccccccccccc"),
                Name             = "Enterprise",
                Description      = "Unlimited scale for large operations.",
                PricePerMonth    = 199.00m,
                MaxUsers         = 0,       // 0 = unlimited
                MaxProducts      = 0,
                MaxWarehouses    = 0,
                MaxMonthlyOrders = 0,
                IsActive         = true,
                CreatedAt        = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                UpdatedAt        = (DateTime?)null
            }
        );
    }
}
