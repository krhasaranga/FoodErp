using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// One ingredient line on a ProductionOrder.
/// PlannedQuantity is calculated from the Recipe when the order is created.
/// ActualQuantity is recorded when production is completed.
/// </summary>
public class ProductionOrderLine : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid ProductionOrderId { get; private set; }
    public ProductionOrder? ProductionOrder { get; private set; }

    public Guid IngredientProductId { get; private set; }
    public Product? IngredientProduct { get; private set; }

    public decimal PlannedQuantity { get; private set; }

    /// <summary>Null until the order is completed.</summary>
    public decimal? ActualQuantity { get; private set; }

    public Guid UnitId { get; private set; }
    public Unit? Unit { get; private set; }

    private ProductionOrderLine() { }

    public static ProductionOrderLine Create(
        Guid productionOrderId,
        Guid ingredientProductId,
        decimal plannedQuantity,
        Guid unitId,
        Guid tenantId)
    {
        return new ProductionOrderLine
        {
            ProductionOrderId   = productionOrderId,
            IngredientProductId = ingredientProductId,
            PlannedQuantity     = plannedQuantity,
            UnitId              = unitId,
            TenantId            = tenantId
        };
    }

    public void RecordActual(decimal actualQuantity)
    {
        if (actualQuantity < 0)
            throw new ArgumentException("Actual quantity cannot be negative.", nameof(actualQuantity));
        ActualQuantity = actualQuantity;
        SetUpdatedAt();
    }
}
