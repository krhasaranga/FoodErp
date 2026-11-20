using FoodErp.Application.DTOs.Admin;
using FoodErp.Application.Interfaces.Admin;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Identity;
using FoodErp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Admin;

public class AdminService : IAdminService
{
    private readonly AppDbContext _db;
    private readonly UserManager<ApplicationUser> _userManager;

    public AdminService(AppDbContext db, UserManager<ApplicationUser> userManager)
    {
        _db          = db;
        _userManager = userManager;
    }

    // ── Monitoring ────────────────────────────────────────────────────────────

    public async Task<PlatformOverviewDto> GetPlatformOverviewAsync()
    {
        var subscriptions = await _db.TenantSubscriptions
            .Include(s => s.Plan)
            .ToListAsync();

        var now              = DateTime.UtcNow;
        var in7Days          = now.AddDays(7);
        var totalTenants     = await _db.Tenants.CountAsync();
        var active           = subscriptions.Count(s => s.Status == SubscriptionStatus.Active);
        var trials           = subscriptions.Count(s => s.Status == SubscriptionStatus.Trial);
        var suspended        = subscriptions.Count(s => s.Status == SubscriptionStatus.Suspended);
        var cancelled        = subscriptions.Count(s => s.Status == SubscriptionStatus.Cancelled);
        var expiringTrials   = subscriptions.Count(s =>
            s.Status == SubscriptionStatus.Trial
            && s.TrialEndsAt.HasValue
            && s.TrialEndsAt.Value <= in7Days);

        var mrr = subscriptions
            .Where(s => s.Status == SubscriptionStatus.Active && s.Plan != null)
            .Sum(s => s.Plan!.PricePerMonth);

        return new PlatformOverviewDto(
            totalTenants, active, trials, suspended, cancelled,
            expiringTrials, mrr);
    }

    public async Task<IEnumerable<TenantOverviewDto>> GetAllTenantsAsync()
    {
        var tenants = await _db.Tenants.ToListAsync();
        var subscriptions = await _db.TenantSubscriptions
            .Include(s => s.Plan)
            .ToListAsync();

        // Count users and products per tenant (bypass global query filter)
        var userCounts = await _db.Users
            .IgnoreQueryFilters()
            .GroupBy(u => u.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToListAsync();

        var productCounts = await _db.Products
            .IgnoreQueryFilters()
            .GroupBy(p => p.TenantId)
            .Select(g => new { TenantId = g.Key, Count = g.Count() })
            .ToListAsync();

        return tenants
            .OrderByDescending(t => t.CreatedAt)
            .Select(t =>
            {
                var sub        = subscriptions.FirstOrDefault(s => s.TenantId == t.Id);
                var userCount  = userCounts.FirstOrDefault(u => u.TenantId == t.Id)?.Count ?? 0;
                var prodCount  = productCounts.FirstOrDefault(p => p.TenantId == t.Id)?.Count ?? 0;

                return new TenantOverviewDto(
                    t.Id, t.Name, t.Slug, t.IsActive,
                    sub?.Plan?.Name ?? "No plan",
                    sub?.Status ?? SubscriptionStatus.Trial,
                    sub?.TrialEndsAt,
                    sub?.CurrentPeriodEnd,
                    userCount, prodCount,
                    t.CreatedAt);
            });
    }

    // ── Subscription Plans ────────────────────────────────────────────────────

    public async Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync()
        => await _db.SubscriptionPlans
            .OrderBy(p => p.PricePerMonth)
            .Select(p => MapPlanToDto(p))
            .ToListAsync();

    public async Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid id)
        => await _db.SubscriptionPlans
            .Where(p => p.Id == id)
            .Select(p => MapPlanToDto(p))
            .FirstOrDefaultAsync();

    public async Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequest request)
    {
        var plan = SubscriptionPlan.Create(
            request.Name, request.PricePerMonth,
            request.MaxUsers, request.MaxProducts,
            request.MaxWarehouses, request.MaxMonthlyOrders,
            request.Description);

        _db.SubscriptionPlans.Add(plan);
        await _db.SaveChangesAsync();

        return await GetPlanByIdAsync(plan.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created plan.");
    }

    public async Task<SubscriptionPlanDto> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request)
    {
        var plan = await _db.SubscriptionPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Subscription plan {id} not found.");

        plan.Update(request.Name, request.Description, request.PricePerMonth,
            request.MaxUsers, request.MaxProducts,
            request.MaxWarehouses, request.MaxMonthlyOrders);

        await _db.SaveChangesAsync();
        return await GetPlanByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated plan.");
    }

    public async Task DeactivatePlanAsync(Guid id)
    {
        var plan = await _db.SubscriptionPlans.FindAsync(id)
            ?? throw new KeyNotFoundException($"Subscription plan {id} not found.");
        plan.Deactivate();
        await _db.SaveChangesAsync();
    }

    // ── Tenant Subscriptions ──────────────────────────────────────────────────

    public async Task<TenantSubscriptionDto?> GetSubscriptionByTenantAsync(Guid tenantId)
    {
        var sub = await _db.TenantSubscriptions
            .Include(s => s.Tenant)
            .Include(s => s.Plan)
            .FirstOrDefaultAsync(s => s.TenantId == tenantId);

        return sub is null ? null : MapSubToDto(sub);
    }

    public async Task<TenantSubscriptionDto> AssignSubscriptionAsync(AssignSubscriptionRequest request)
    {
        var existing = await _db.TenantSubscriptions
            .AnyAsync(s => s.TenantId == request.TenantId);

        if (existing)
            throw new InvalidOperationException(
                "Tenant already has a subscription. Use ChangePlan or Renew instead.");

        TenantSubscription sub;

        if (request.StartTrial)
            sub = TenantSubscription.CreateTrial(request.TenantId, request.PlanId, request.TrialDays);
        else
            sub = TenantSubscription.CreateActive(
                request.TenantId, request.PlanId,
                request.PeriodStart ?? DateTime.UtcNow,
                request.PeriodEnd   ?? DateTime.UtcNow.AddMonths(1));

        _db.TenantSubscriptions.Add(sub);
        await _db.SaveChangesAsync();

        return await GetSubscriptionByTenantAsync(request.TenantId)
            ?? throw new InvalidOperationException("Failed to retrieve created subscription.");
    }

    public async Task<TenantSubscriptionDto> ChangePlanAsync(Guid tenantId, ChangePlanRequest request)
    {
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {tenantId}.");
        sub.ChangePlan(request.NewPlanId);
        await _db.SaveChangesAsync();
        return await GetSubscriptionByTenantAsync(tenantId)
            ?? throw new InvalidOperationException("Failed to retrieve subscription.");
    }

    public async Task<TenantSubscriptionDto> RenewAsync(Guid tenantId, RenewSubscriptionRequest request)
    {
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {tenantId}.");
        sub.Renew(request.NewPeriodStart, request.NewPeriodEnd);
        await _db.SaveChangesAsync();
        return await GetSubscriptionByTenantAsync(tenantId)
            ?? throw new InvalidOperationException("Failed to retrieve subscription.");
    }

    public async Task<TenantSubscriptionDto> SuspendAsync(Guid tenantId, SuspendSubscriptionRequest request)
    {
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {tenantId}.");
        sub.Suspend(request.Reason);
        await _db.SaveChangesAsync();
        return await GetSubscriptionByTenantAsync(tenantId)
            ?? throw new InvalidOperationException("Failed to retrieve subscription.");
    }

    public async Task<TenantSubscriptionDto> CancelAsync(Guid tenantId, CancelSubscriptionRequest request)
    {
        var sub = await _db.TenantSubscriptions.FirstOrDefaultAsync(s => s.TenantId == tenantId)
            ?? throw new KeyNotFoundException($"No subscription found for tenant {tenantId}.");
        sub.Cancel(request.Reason);
        await _db.SaveChangesAsync();
        return await GetSubscriptionByTenantAsync(tenantId)
            ?? throw new InvalidOperationException("Failed to retrieve subscription.");
    }

    // ── Mappers ───────────────────────────────────────────────────────────────

    private static SubscriptionPlanDto MapPlanToDto(SubscriptionPlan p) => new(
        p.Id, p.Name, p.Description, p.PricePerMonth,
        p.MaxUsers, p.MaxProducts, p.MaxWarehouses, p.MaxMonthlyOrders,
        p.IsActive, p.CreatedAt);

    private static TenantSubscriptionDto MapSubToDto(TenantSubscription s) => new(
        s.Id,
        s.TenantId, s.Tenant!.Name, s.Tenant.Slug,
        s.PlanId, s.Plan!.Name, s.Plan.PricePerMonth,
        s.Status,
        s.StartDate, s.TrialEndsAt,
        s.CurrentPeriodStart, s.CurrentPeriodEnd,
        s.SuspendedAt, s.CancelledAt,
        s.Notes, s.CreatedAt);
}
