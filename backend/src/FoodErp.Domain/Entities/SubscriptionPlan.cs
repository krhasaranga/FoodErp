using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum SubscriptionStatus
{
    Trial,       // free trial period
    Active,      // paying and in good standing
    PastDue,     // payment overdue but access not yet cut
    Suspended,   // access restricted due to non-payment
    Cancelled    // tenant has churned
}

/// <summary>
/// Defines a subscription tier available on the platform.
/// Global entity — not tenant-scoped.
/// 0 on a limit field means "unlimited".
/// </summary>
public class SubscriptionPlan : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }

    /// <summary>Monthly price in the platform's base currency.</summary>
    public decimal PricePerMonth { get; private set; }

    // Limits — enforced by the application layer
    public int MaxUsers { get; private set; }
    public int MaxProducts { get; private set; }
    public int MaxWarehouses { get; private set; }

    /// <summary>0 = unlimited sales orders per month.</summary>
    public int MaxMonthlyOrders { get; private set; }

    public bool IsActive { get; private set; } = true;

    private SubscriptionPlan() { }

    public static SubscriptionPlan Create(
        string name,
        decimal pricePerMonth,
        int maxUsers,
        int maxProducts,
        int maxWarehouses,
        int maxMonthlyOrders = 0,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (pricePerMonth < 0)
            throw new ArgumentException("Price cannot be negative.", nameof(pricePerMonth));

        return new SubscriptionPlan
        {
            Name              = name.Trim(),
            Description       = description?.Trim(),
            PricePerMonth     = pricePerMonth,
            MaxUsers          = maxUsers,
            MaxProducts       = maxProducts,
            MaxWarehouses     = maxWarehouses,
            MaxMonthlyOrders  = maxMonthlyOrders
        };
    }

    public void Update(
        string name,
        string? description,
        decimal pricePerMonth,
        int maxUsers,
        int maxProducts,
        int maxWarehouses,
        int maxMonthlyOrders)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name             = name.Trim();
        Description      = description?.Trim();
        PricePerMonth    = pricePerMonth;
        MaxUsers         = maxUsers;
        MaxProducts      = maxProducts;
        MaxWarehouses    = maxWarehouses;
        MaxMonthlyOrders = maxMonthlyOrders;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
