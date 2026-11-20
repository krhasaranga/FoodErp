using FoodErp.Application.DTOs.Admin;

namespace FoodErp.Application.Interfaces.Admin;

public interface IAdminService
{
    // ── Monitoring ────────────────────────────────────────────────────────────
    Task<PlatformOverviewDto> GetPlatformOverviewAsync();
    Task<IEnumerable<TenantOverviewDto>> GetAllTenantsAsync();

    // ── Subscription Plans ────────────────────────────────────────────────────
    Task<IEnumerable<SubscriptionPlanDto>> GetPlansAsync();
    Task<SubscriptionPlanDto?> GetPlanByIdAsync(Guid id);
    Task<SubscriptionPlanDto> CreatePlanAsync(CreateSubscriptionPlanRequest request);
    Task<SubscriptionPlanDto> UpdatePlanAsync(Guid id, UpdateSubscriptionPlanRequest request);
    Task DeactivatePlanAsync(Guid id);

    // ── Tenant Subscriptions ──────────────────────────────────────────────────
    Task<TenantSubscriptionDto?> GetSubscriptionByTenantAsync(Guid tenantId);
    Task<TenantSubscriptionDto> AssignSubscriptionAsync(AssignSubscriptionRequest request);
    Task<TenantSubscriptionDto> ChangePlanAsync(Guid tenantId, ChangePlanRequest request);
    Task<TenantSubscriptionDto> RenewAsync(Guid tenantId, RenewSubscriptionRequest request);
    Task<TenantSubscriptionDto> SuspendAsync(Guid tenantId, SuspendSubscriptionRequest request);
    Task<TenantSubscriptionDto> CancelAsync(Guid tenantId, CancelSubscriptionRequest request);
}
