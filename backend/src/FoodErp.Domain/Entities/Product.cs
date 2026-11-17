using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum ProductType
{
    FinishedGood,       // manufactured and sold
    RawMaterial,        // used in production
    Packaging,          // packaging materials
    SemiFinished        // intermediate product
}

public class Product : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Code { get; private set; } = string.Empty;        // e.g. "CAKE-001"
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public ProductType Type { get; private set; }
    public Guid CategoryId { get; private set; }
    public ProductCategory? Category { get; private set; }
    public Guid UnitId { get; private set; }                        // selling/stock unit
    public Unit? Unit { get; private set; }
    public decimal CostPrice { get; private set; }
    public decimal SellingPrice { get; private set; }
    public decimal ReorderLevel { get; private set; }               // trigger restocking alert
    public bool TrackExpiry { get; private set; }
    public bool IsActive { get; private set; } = true;

    private Product() { }

    public static Product Create(
        string code,
        string name,
        ProductType type,
        Guid categoryId,
        Guid unitId,
        Guid tenantId,
        decimal costPrice = 0,
        decimal sellingPrice = 0,
        decimal reorderLevel = 0,
        bool trackExpiry = false,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(code);
        ArgumentException.ThrowIfNullOrWhiteSpace(name);

        if (costPrice < 0) throw new ArgumentException("Cost price cannot be negative.");
        if (sellingPrice < 0) throw new ArgumentException("Selling price cannot be negative.");

        return new Product
        {
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Type = type,
            CategoryId = categoryId,
            UnitId = unitId,
            TenantId = tenantId,
            CostPrice = costPrice,
            SellingPrice = sellingPrice,
            ReorderLevel = reorderLevel,
            TrackExpiry = trackExpiry,
            Description = description?.Trim()
        };
    }

    public void UpdatePrices(decimal costPrice, decimal sellingPrice)
    {
        if (costPrice < 0) throw new ArgumentException("Cost price cannot be negative.");
        if (sellingPrice < 0) throw new ArgumentException("Selling price cannot be negative.");
        CostPrice = costPrice;
        SellingPrice = sellingPrice;
        SetUpdatedAt();
    }

    public void Update(string code, string name, string? description,
        Guid categoryId, Guid unitId, decimal reorderLevel, bool trackExpiry)
    {
        Code = code.Trim().ToUpperInvariant();
        Name = name.Trim();
        Description = description?.Trim();
        CategoryId = categoryId;
        UnitId = unitId;
        ReorderLevel = reorderLevel;
        TrackExpiry = trackExpiry;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}