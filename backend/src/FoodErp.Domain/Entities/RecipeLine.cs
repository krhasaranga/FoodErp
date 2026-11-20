using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// A single ingredient line in a Recipe.
/// Quantity is per ONE batch (i.e. per Recipe.OutputQuantity units of output).
/// </summary>
public class RecipeLine : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    public Guid IngredientProductId { get; private set; }
    public Product? IngredientProduct { get; private set; }

    /// <summary>Quantity of this ingredient needed per batch.</summary>
    public decimal Quantity { get; private set; }
    public Guid UnitId { get; private set; }
    public Unit? Unit { get; private set; }

    public string? Notes { get; private set; }

    private RecipeLine() { }

    public static RecipeLine Create(
        Guid recipeId,
        Guid ingredientProductId,
        decimal quantity,
        Guid unitId,
        Guid tenantId,
        string? notes = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Ingredient quantity must be greater than zero.", nameof(quantity));

        return new RecipeLine
        {
            RecipeId              = recipeId,
            IngredientProductId   = ingredientProductId,
            Quantity              = quantity,
            UnitId                = unitId,
            TenantId              = tenantId,
            Notes                 = notes?.Trim()
        };
    }

    public void Update(decimal quantity, Guid unitId, string? notes)
    {
        if (quantity <= 0)
            throw new ArgumentException("Ingredient quantity must be greater than zero.", nameof(quantity));

        Quantity = quantity;
        UnitId   = unitId;
        Notes    = notes?.Trim();
        SetUpdatedAt();
    }
}
