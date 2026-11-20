using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// A Bill of Materials (recipe) for producing a finished or semi-finished product.
/// Defines what ingredients are needed and in what quantities per batch.
/// </summary>
public class Recipe : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>The finished good or semi-finished product this recipe produces.</summary>
    public Guid OutputProductId { get; private set; }
    public Product? OutputProduct { get; private set; }

    /// <summary>
    /// How many units the recipe yields per batch.
    /// e.g. OutputQuantity=12, OutputProduct=Cake Slice → one batch makes 12 slices.
    /// </summary>
    public decimal OutputQuantity { get; private set; }
    public Guid OutputUnitId { get; private set; }
    public Unit? OutputUnit { get; private set; }

    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; } = true;

    public ICollection<RecipeLine> Lines { get; private set; } = new List<RecipeLine>();

    private Recipe() { }

    public static Recipe Create(
        string name,
        Guid outputProductId,
        decimal outputQuantity,
        Guid outputUnitId,
        Guid tenantId,
        string? description = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (outputQuantity <= 0)
            throw new ArgumentException("Output quantity must be greater than zero.", nameof(outputQuantity));

        return new Recipe
        {
            Name            = name.Trim(),
            OutputProductId = outputProductId,
            OutputQuantity  = outputQuantity,
            OutputUnitId    = outputUnitId,
            TenantId        = tenantId,
            Description     = description?.Trim()
        };
    }

    public void Update(string name, string? description, decimal outputQuantity, Guid outputUnitId)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        if (outputQuantity <= 0)
            throw new ArgumentException("Output quantity must be greater than zero.", nameof(outputQuantity));

        Name           = name.Trim();
        Description    = description?.Trim();
        OutputQuantity = outputQuantity;
        OutputUnitId   = outputUnitId;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}
