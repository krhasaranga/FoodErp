using FoodErp.Application.Interfaces;
using FoodErp.Domain.Common;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Persistence;

public class AppDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    private readonly ITenantContext _tenantContext;

    public AppDbContext(DbContextOptions<AppDbContext> options, ITenantContext tenantContext)
        : base(options)
    {
        _tenantContext = tenantContext;
    }

    public DbSet<Tenant> Tenants => Set<Tenant>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<Unit> Units => Set<Unit>();                                    
    public DbSet<UnitConversion> UnitConversions => Set<UnitConversion>();      
    public DbSet<Product> Products => Set<Product>();                         

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder); // applies Identity table configurations

        // Apply all IEntityTypeConfiguration<T> classes in this assembly
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);

        // Apply global tenant query filter to every entity implementing ITenantEntity
        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (typeof(ITenantEntity).IsAssignableFrom(entityType.ClrType))
            {
                modelBuilder.Entity(entityType.ClrType)
                    .AddTenantQueryFilter(_tenantContext);
            }
        }
    }
}