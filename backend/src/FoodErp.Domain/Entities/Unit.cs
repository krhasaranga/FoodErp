using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// A unit of measure used for products and inventory.
/// e.g. Kilogram, Gram, Piece, Box, Carton
/// </summary>
public class Unit : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;       // "Kilogram"
    public string Symbol { get; private set; } = string.Empty;     // "kg"
    public bool IsBaseUnit { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Unit() { }

    public static Unit Create(string name, string symbol, Guid tenantId, bool isBaseUnit = false)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(symbol);
        return new Unit
        {
            Name = name.Trim(),
            Symbol = symbol.Trim(),
            TenantId = tenantId,
            IsBaseUnit = isBaseUnit
        };
    }

    public void Update(string name, string symbol)
    {
        Name = name.Trim();
        Symbol = symbol.Trim();
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}