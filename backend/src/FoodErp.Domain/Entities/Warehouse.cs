using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// A physical or logical storage location belonging to a tenant.
/// e.g. "Main Warehouse", "Cold Room", "Retail Store A"
/// </summary>
public class Warehouse : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Location { get; private set; }   // free-text address or area
    public bool IsActive { get; private set; } = true;

    // Navigation
    public ICollection<StockBalance> StockBalances { get; private set; } = new List<StockBalance>();
    public ICollection<StockMovement> StockMovements { get; private set; } = new List<StockMovement>();

    private Warehouse() { }

    public static Warehouse Create(string name, Guid tenantId, string? location = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new Warehouse
        {
            Name     = name.Trim(),
            TenantId = tenantId,
            Location = location?.Trim()
        };
    }

    public void Update(string name, string? location)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        Name     = name.Trim();
        Location = location?.Trim();
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
