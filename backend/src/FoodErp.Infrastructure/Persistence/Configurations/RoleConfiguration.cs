using FoodErp.Infrastructure.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FoodErp.Infrastructure.Persistence.Configurations;

public class RoleConfiguration : IEntityTypeConfiguration<ApplicationRole>
{
    public void Configure(EntityTypeBuilder<ApplicationRole> builder)
    {
        builder.HasData(
            new ApplicationRole
            {
                Id = Guid.Parse("11111111-1111-1111-1111-111111111111"),
                Name = "TenantAdmin",
                NormalizedName = "TENANTADMIN",
                ConcurrencyStamp = "1"
            },
            new ApplicationRole
            {
                Id = Guid.Parse("22222222-2222-2222-2222-222222222222"),
                Name = "TenantUser",
                NormalizedName = "TENANTUSER",
                ConcurrencyStamp = "2"
            }
        );
    }
}