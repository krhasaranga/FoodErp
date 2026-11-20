using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum CustomerType
{
    Retail,       // individual shop or end consumer
    Wholesale,    // bulk buyer
    Distributor   // resells to other retailers
}

/// <summary>
/// A business or individual that purchases products from the tenant.
/// </summary>
public class Customer : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Short unique code, e.g. CUST-001.</summary>
    public string Code { get; private set; } = string.Empty;

    public string Name { get; private set; } = string.Empty;
    public CustomerType Type { get; private set; }

    public string? ContactPerson { get; private set; }
    public string? Phone { get; private set; }
    public string? Email { get; private set; }
    public string? Address { get; private set; }

    /// <summary>Credit limit in the tenant's currency. Null = no limit enforced.</summary>
    public decimal? CreditLimit { get; private set; }

    public bool IsActive { get; private set; } = true;

    public ICollection<SalesOrder> SalesOrders { get; private set; } = new List<SalesOrder>();

    private Customer() { }

    public static Customer Create(
        string code,
        string name,
        CustomerType type,
        Guid tenantId,
        string? contactPerson = null,
        string? phone = null,
        string? email = null,
        string? address = null,
        decimal? creditLimit = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        return new Customer
        {
            Code          = code.Trim().ToUpperInvariant(),
            Name          = name.Trim(),
            Type          = type,
            TenantId      = tenantId,
            ContactPerson = contactPerson?.Trim(),
            Phone         = phone?.Trim(),
            Email         = email?.Trim().ToLowerInvariant(),
            Address       = address?.Trim(),
            CreditLimit   = creditLimit
        };
    }

    public void Update(
        string name,
        CustomerType type,
        string? contactPerson,
        string? phone,
        string? email,
        string? address,
        decimal? creditLimit)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name          = name.Trim();
        Type          = type;
        ContactPerson = contactPerson?.Trim();
        Phone         = phone?.Trim();
        Email         = email?.Trim().ToLowerInvariant();
        Address       = address?.Trim();
        CreditLimit   = creditLimit;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
