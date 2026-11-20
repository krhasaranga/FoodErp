using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Admin;

// ── Subscription Plans ────────────────────────────────────────────────────────

public record SubscriptionPlanDto(
    Guid Id,
    string Name,
    string? Description,
    decimal PricePerMonth,
    int MaxUsers,
    int MaxProducts,
    int MaxWarehouses,
    int MaxMonthlyOrders,
    bool IsActive,
    DateTime CreatedAt);

public record CreateSubscriptionPlanRequest(
    string Name,
    string? Description,
    decimal PricePerMonth,
    int MaxUsers,
    int MaxProducts,
    int MaxWarehouses,
    int MaxMonthlyOrders = 0);

public record UpdateSubscriptionPlanRequest(
    string Name,
    string? Description,
    decimal PricePerMonth,
    int MaxUsers,
    int MaxProducts,
    int MaxWarehouses,
    int MaxMonthlyOrders);

// ── Tenant Subscriptions ──────────────────────────────────────────────────────

public record TenantSubscriptionDto(
    Guid Id,
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    Guid PlanId,
    string PlanName,
    decimal PlanPrice,
    SubscriptionStatus Status,
    DateTime StartDate,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodStart,
    DateTime? CurrentPeriodEnd,
    DateTime? SuspendedAt,
    DateTime? CancelledAt,
    string? Notes,
    DateTime CreatedAt);

public record AssignSubscriptionRequest(
    Guid TenantId,
    Guid PlanId,
    bool StartTrial = true,
    int TrialDays = 14,
    DateTime? PeriodStart = null,
    DateTime? PeriodEnd = null);

public record ChangePlanRequest(Guid NewPlanId);

public record SuspendSubscriptionRequest(string? Reason);

public record CancelSubscriptionRequest(string? Reason);

public record RenewSubscriptionRequest(
    DateTime NewPeriodStart,
    DateTime NewPeriodEnd);

// ── Platform Monitoring ───────────────────────────────────────────────────────

public record PlatformOverviewDto(
    int TotalTenants,
    int ActiveSubscriptions,
    int TrialSubscriptions,
    int SuspendedSubscriptions,
    int CancelledSubscriptions,
    int TrialsExpiringIn7Days,
    decimal MonthlyRecurringRevenue);

public record TenantOverviewDto(
    Guid TenantId,
    string TenantName,
    string TenantSlug,
    bool TenantIsActive,
    string PlanName,
    SubscriptionStatus SubscriptionStatus,
    DateTime? TrialEndsAt,
    DateTime? CurrentPeriodEnd,
    int UserCount,
    int ProductCount,
    DateTime TenantCreatedAt);
