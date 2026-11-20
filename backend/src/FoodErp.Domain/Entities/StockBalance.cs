using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// The current on-hand quantity for a specific product in a specific warehouse.
/// Updated atomically alongside every StockMovement inside a transaction.
/// One row per (TenantId, WarehouseId, ProductId) — enforced by a unique index.
/// </summary>
public class StockBalance : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid WarehouseId { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public decimal Quantity { get; private set; }

    private StockBalance() { }

    /// <summary>Creates a brand-new balance row (quantity starts at zero).</summary>
    public static StockBalance CreateNew(Guid tenantId, Guid warehouseId, Guid productId)
        => new()
        {
            TenantId    = tenantId,
            WarehouseId = warehouseId,
            ProductId   = productId,
            Quantity    = 0
        };

    /// <summary>
    /// Adds to the balance. Used for Receive, TransferIn, AdjustUp, OpeningStock.
    /// </summary>
    public void Increase(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Increase amount must be positive.", nameof(amount));
        Quantity += amount;
        SetUpdatedAt();
    }

    /// <summary>
    /// Subtracts from the balance. Throws if the result would go negative.
    /// Used for Issue, TransferOut, AdjustDown.
    /// </summary>
    public void Decrease(decimal amount)
    {
        if (amount <= 0)
            throw new ArgumentException("Decrease amount must be positive.", nameof(amount));
        if (Quantity - amount < 0)
            throw new InvalidOperationException(
                $"Insufficient stock. Available: {Quantity}, requested: {amount}.");
        Quantity -= amount;
        SetUpdatedAt();
    }
}
