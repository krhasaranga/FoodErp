using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// The type of stock movement — drives how balances are updated.
/// </summary>
public enum MovementType
{
    OpeningStock,   // initial stock loaded when setting up a warehouse
    Receive,        // stock received (from purchase, production output, return)
    Issue,          // stock consumed (for production, sale, internal use)
    AdjustUp,       // positive manual correction
    AdjustDown,     // negative manual correction
    TransferOut,    // leaving source warehouse (always paired with TransferIn)
    TransferIn      // arriving at destination warehouse (always paired with TransferOut)
}

/// <summary>
/// An immutable record of every stock change in every warehouse.
/// This is the audit trail — movements are NEVER deleted or edited.
/// </summary>
public class StockMovement : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    // Where the movement happened
    public Guid WarehouseId { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    // What moved
    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public MovementType Type { get; private set; }

    /// <summary>
    /// Always positive. Direction is determined by MovementType.
    /// </summary>
    public decimal Quantity { get; private set; }

    /// <summary>
    /// Balance in this warehouse AFTER this movement was applied.
    /// Stored for quick reporting — no need to replay all history.
    /// </summary>
    public decimal BalanceAfter { get; private set; }

    // Optional traceability
    public string? Reference { get; private set; }   // PO#, production order#, transfer#
    public string? Notes { get; private set; }

    // Links the TransferOut + TransferIn pair created in a single transfer
    public Guid? TransferPairId { get; private set; }

    // Batch / expiry tracking (populated only when Product.TrackExpiry = true)
    public string? BatchNumber { get; private set; }
    public DateOnly? ExpiryDate { get; private set; }

    // When the physical movement happened (may differ from CreatedAt)
    public DateTime MovedAt { get; private set; }

    private StockMovement() { }

    public static StockMovement Create(
        Guid tenantId,
        Guid warehouseId,
        Guid productId,
        MovementType type,
        decimal quantity,
        decimal balanceAfter,
        DateTime movedAt,
        string? reference = null,
        string? notes = null,
        Guid? transferPairId = null,
        string? batchNumber = null,
        DateOnly? expiryDate = null)
    {
        if (quantity <= 0)
            throw new ArgumentException("Movement quantity must be greater than zero.", nameof(quantity));

        return new StockMovement
        {
            TenantId       = tenantId,
            WarehouseId    = warehouseId,
            ProductId      = productId,
            Type           = type,
            Quantity       = quantity,
            BalanceAfter   = balanceAfter,
            MovedAt        = movedAt,
            Reference      = reference?.Trim(),
            Notes          = notes?.Trim(),
            TransferPairId = transferPairId,
            BatchNumber    = batchNumber?.Trim(),
            ExpiryDate     = expiryDate
        };
    }
}
