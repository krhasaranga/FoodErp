using FoodErp.Application.DTOs.Inventory;

namespace FoodErp.Application.Interfaces.Inventory;

public interface IStockService
{
    // ── Queries ──────────────────────────────────────────────────────────────

    /// <summary>All current balances for a warehouse.</summary>
    Task<IEnumerable<StockBalanceDto>> GetBalancesByWarehouseAsync(Guid warehouseId);

    /// <summary>Balance of a single product across all warehouses.</summary>
    Task<IEnumerable<StockBalanceDto>> GetBalancesByProductAsync(Guid productId);

    /// <summary>Movement history (newest first). Pass warehouseId and/or productId to filter.</summary>
    Task<IEnumerable<StockMovementDto>> GetMovementsAsync(
        Guid? warehouseId, Guid? productId, int page = 1, int pageSize = 50);

    // ── Mutations ─────────────────────────────────────────────────────────────

    Task<StockMovementDto> SetOpeningStockAsync(OpeningStockRequest request);
    Task<StockMovementDto> ReceiveAsync(ReceiveStockRequest request);
    Task<StockMovementDto> IssueAsync(IssueStockRequest request);
    Task<StockMovementDto> AdjustAsync(AdjustStockRequest request);

    /// <summary>Atomic transfer between warehouses. Returns both movements.</summary>
    Task<(StockMovementDto Out, StockMovementDto In)> TransferAsync(TransferStockRequest request);
}
