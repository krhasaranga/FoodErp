using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// Links a Tenant to a SubscriptionPlan and tracks billing status.
/// Global entity — not tenant-scoped (platform admin manages this).
/// One subscription per tenant (enforced by unique index on TenantId).
/// </summary>
public class TenantSubscription : BaseEntity
{
    public Guid TenantId { get; private set; }
    public Tenant? Tenant { get; private set; }

    public Guid PlanId { get; private set; }
    public SubscriptionPlan? Plan { get; private set; }

    public SubscriptionStatus Status { get; private set; } = SubscriptionStatus.Trial;

    public DateTime StartDate { get; private set; }

    /// <summary>When the trial expires. Null if not on trial.</summary>
    public DateTime? TrialEndsAt { get; private set; }

    public DateTime? CurrentPeriodStart { get; private set; }
    public DateTime? CurrentPeriodEnd { get; private set; }

    public DateTime? SuspendedAt { get; private set; }
    public DateTime? CancelledAt { get; private set; }

    public string? Notes { get; private set; }

    private TenantSubscription() { }

    public static TenantSubscription CreateTrial(
        Guid tenantId,
        Guid planId,
        int trialDays = 14)
    {
        var now = DateTime.UtcNow;
        return new TenantSubscription
        {
            TenantId     = tenantId,
            PlanId       = planId,
            Status       = SubscriptionStatus.Trial,
            StartDate    = now,
            TrialEndsAt  = now.AddDays(trialDays)
        };
    }

    public static TenantSubscription CreateActive(
        Guid tenantId,
        Guid planId,
        DateTime periodStart,
        DateTime periodEnd)
    {
        return new TenantSubscription
        {
            TenantId           = tenantId,
            PlanId             = planId,
            Status             = SubscriptionStatus.Active,
            StartDate          = periodStart.ToUniversalTime(),
            CurrentPeriodStart = periodStart.ToUniversalTime(),
            CurrentPeriodEnd   = periodEnd.ToUniversalTime()
        };
    }

    public void Activate(Guid planId, DateTime periodStart, DateTime periodEnd)
    {
        PlanId             = planId;
        Status             = SubscriptionStatus.Active;
        CurrentPeriodStart = periodStart.ToUniversalTime();
        CurrentPeriodEnd   = periodEnd.ToUniversalTime();
        TrialEndsAt        = null;
        SuspendedAt        = null;
        SetUpdatedAt();
    }

    public void Renew(DateTime newPeriodStart, DateTime newPeriodEnd)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot renew a cancelled subscription.");

        Status             = SubscriptionStatus.Active;
        CurrentPeriodStart = newPeriodStart.ToUniversalTime();
        CurrentPeriodEnd   = newPeriodEnd.ToUniversalTime();
        SuspendedAt        = null;
        SetUpdatedAt();
    }

    public void Suspend(string? reason = null)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot suspend a cancelled subscription.");

        Status      = SubscriptionStatus.Suspended;
        SuspendedAt = DateTime.UtcNow;
        Notes       = reason;
        SetUpdatedAt();
    }

    public void Cancel(string? reason = null)
    {
        Status      = SubscriptionStatus.Cancelled;
        CancelledAt = DateTime.UtcNow;
        Notes       = reason;
        SetUpdatedAt();
    }

    public void ChangePlan(Guid newPlanId)
    {
        if (Status == SubscriptionStatus.Cancelled)
            throw new InvalidOperationException("Cannot change plan on a cancelled subscription.");

        PlanId = newPlanId;
        SetUpdatedAt();
    }
}
