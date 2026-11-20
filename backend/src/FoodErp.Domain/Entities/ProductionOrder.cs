using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum ProductionOrderStatus
{
    Draft,       // created but not started
    InProgress,  // production has started, ingredients committed
    Completed,   // finished goods received, ingredients issued
    Cancelled    // abandoned before completion
}

/// <summary>
/// An instruction to produce a specific quantity of a product using a Recipe.
/// Tracks planned vs actual quantities and drives stock movements on completion.
/// </summary>
public class ProductionOrder : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Auto-generated human-readable reference, e.g. PRD-2026-001.</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid RecipeId { get; private set; }
    public Recipe? Recipe { get; private set; }

    /// <summary>Warehouse where raw ingredients are taken from.</summary>
    public Guid InputWarehouseId { get; private set; }
    public Warehouse? InputWarehouse { get; private set; }

    /// <summary>Warehouse where finished goods are received into.</summary>
    public Guid OutputWarehouseId { get; private set; }
    public Warehouse? OutputWarehouse { get; private set; }

    /// <summary>
    /// Number of batches planned.
    /// Total ingredient requirement = RecipeLine.Quantity × PlannedBatches.
    /// Total output = Recipe.OutputQuantity × PlannedBatches.
    /// </summary>
    public decimal PlannedBatches { get; private set; }

    /// <summary>Actual batches completed. Set on completion.</summary>
    public decimal? ActualBatches { get; private set; }

    public ProductionOrderStatus Status { get; private set; } = ProductionOrderStatus.Draft;

    public DateTime PlannedDate { get; private set; }
    public DateTime? StartedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public string? Notes { get; private set; }

    public ICollection<ProductionOrderLine> Lines { get; private set; } = new List<ProductionOrderLine>();

    private ProductionOrder() { }

    public static ProductionOrder Create(
        string orderNumber,
        Guid recipeId,
        Guid inputWarehouseId,
        Guid outputWarehouseId,
        decimal plannedBatches,
        DateTime plannedDate,
        Guid tenantId,
        string? notes = null)
    {
        if (plannedBatches <= 0)
            throw new ArgumentException("Planned batches must be greater than zero.", nameof(plannedBatches));

        return new ProductionOrder
        {
            OrderNumber       = orderNumber,
            RecipeId          = recipeId,
            InputWarehouseId  = inputWarehouseId,
            OutputWarehouseId = outputWarehouseId,
            PlannedBatches    = plannedBatches,
            PlannedDate       = plannedDate.ToUniversalTime(),
            TenantId          = tenantId,
            Notes             = notes?.Trim()
        };
    }

    public void Start()
    {
        if (Status != ProductionOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot start a production order in '{Status}' status.");

        Status    = ProductionOrderStatus.InProgress;
        StartedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Complete(decimal actualBatches)
    {
        if (Status != ProductionOrderStatus.InProgress)
            throw new InvalidOperationException($"Cannot complete a production order in '{Status}' status.");
        if (actualBatches <= 0)
            throw new ArgumentException("Actual batches must be greater than zero.", nameof(actualBatches));

        Status        = ProductionOrderStatus.Completed;
        ActualBatches = actualBatches;
        CompletedAt   = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == ProductionOrderStatus.Completed)
            throw new InvalidOperationException("A completed production order cannot be cancelled.");

        Status = ProductionOrderStatus.Cancelled;
        SetUpdatedAt();
    }

    public void UpdateNotes(string? notes)
    {
        Notes = notes?.Trim();
        SetUpdatedAt();
    }
}
