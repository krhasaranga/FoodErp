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

    // Inventory
    public DbSet<Warehouse> Warehouses => Set<Warehouse>();
    public DbSet<StockBalance> StockBalances => Set<StockBalance>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();

    // Production
    public DbSet<Recipe> Recipes => Set<Recipe>();
    public DbSet<RecipeLine> RecipeLines => Set<RecipeLine>();
    public DbSet<ProductionOrder> ProductionOrders => Set<ProductionOrder>();
    public DbSet<ProductionOrderLine> ProductionOrderLines => Set<ProductionOrderLine>();

    // Distribution
    public DbSet<Customer> Customers => Set<Customer>();
    public DbSet<SalesOrder> SalesOrders => Set<SalesOrder>();
    public DbSet<SalesOrderLine> SalesOrderLines => Set<SalesOrderLine>();

    // Invoicing
    public DbSet<Invoice> Invoices => Set<Invoice>();
    public DbSet<InvoiceLine> InvoiceLines => Set<InvoiceLine>();
    public DbSet<Payment> Payments => Set<Payment>();

    // Employees
    public DbSet<Employee> Employees => Set<Employee>();
    public DbSet<Attendance> Attendance => Set<Attendance>();

    // SaaS Administration (global — no tenant filter)
    public DbSet<SubscriptionPlan> SubscriptionPlans => Set<SubscriptionPlan>();
    public DbSet<TenantSubscription> TenantSubscriptions => Set<TenantSubscription>();

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