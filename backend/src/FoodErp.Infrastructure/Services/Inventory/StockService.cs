using FoodErp.Application.DTOs.Inventory;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Inventory;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Inventory;

public class StockService : IStockService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public StockService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    // ── Queries ───────────────────────────────────────────────────────────────

    public async Task<IEnumerable<StockBalanceDto>> GetBalancesByWarehouseAsync(Guid warehouseId)
        => await _db.StockBalances
            .Where(b => b.WarehouseId == warehouseId)
            .Include(b => b.Product).ThenInclude(p => p!.Unit)
            .Include(b => b.Warehouse)
            .OrderBy(b => b.Product!.Name)
            .Select(b => new StockBalanceDto(
                b.ProductId,
                b.Product!.Code,
                b.Product.Name,
                b.Product.Unit!.Symbol,
                b.WarehouseId,
                b.Warehouse!.Name,
                b.Quantity))
            .ToListAsync();

    public async Task<IEnumerable<StockBalanceDto>> GetBalancesByProductAsync(Guid productId)
        => await _db.StockBalances
            .Where(b => b.ProductId == productId)
            .Include(b => b.Product).ThenInclude(p => p!.Unit)
            .Include(b => b.Warehouse)
            .OrderBy(b => b.Warehouse!.Name)
            .Select(b => new StockBalanceDto(
                b.ProductId,
                b.Product!.Code,
                b.Product.Name,
                b.Product.Unit!.Symbol,
                b.WarehouseId,
                b.Warehouse!.Name,
                b.Quantity))
            .ToListAsync();

    public async Task<IEnumerable<StockMovementDto>> GetMovementsAsync(
        Guid? warehouseId, Guid? productId, int page = 1, int pageSize = 50)
    {
        var query = _db.StockMovements
            .Include(m => m.Warehouse)
            .Include(m => m.Product)
            .AsQueryable();

        if (warehouseId.HasValue) query = query.Where(m => m.WarehouseId == warehouseId);
        if (productId.HasValue)   query = query.Where(m => m.ProductId == productId);

        var items = await query
            .OrderByDescending(m => m.MovedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return items.Select(MapMovement);
    }

    // ── Mutations ─────────────────────────────────────────────────────────────

    public async Task<StockMovementDto> SetOpeningStockAsync(OpeningStockRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        // Opening stock can only be set once per product per warehouse
        var exists = await _db.StockMovements.AnyAsync(
            m => m.WarehouseId == request.WarehouseId
              && m.ProductId   == request.ProductId
              && m.Type        == MovementType.OpeningStock);

        if (exists)
            throw new InvalidOperationException(
                "Opening stock has already been set for this product in this warehouse. Use an adjustment instead.");

        var balance = await GetOrCreateBalanceAsync(request.WarehouseId, request.ProductId);
        balance.Increase(request.Quantity);

        var movement = StockMovement.Create(
            _tenant.TenantId, request.WarehouseId, request.ProductId,
            MovementType.OpeningStock, request.Quantity, balance.Quantity,
            DateTime.UtcNow,
            notes: request.Notes,
            batchNumber: request.BatchNumber,
            expiryDate: request.ExpiryDate);

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await LoadMovementDto(movement.Id);
    }

    public async Task<StockMovementDto> ReceiveAsync(ReceiveStockRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var balance = await GetOrCreateBalanceAsync(request.WarehouseId, request.ProductId);
        balance.Increase(request.Quantity);

        var movement = StockMovement.Create(
            _tenant.TenantId, request.WarehouseId, request.ProductId,
            MovementType.Receive, request.Quantity, balance.Quantity,
            request.MovedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            reference:   request.Reference,
            notes:       request.Notes,
            batchNumber: request.BatchNumber,
            expiryDate:  request.ExpiryDate);

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await LoadMovementDto(movement.Id);
    }

    public async Task<StockMovementDto> IssueAsync(IssueStockRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var balance = await GetOrCreateBalanceAsync(request.WarehouseId, request.ProductId);
        balance.Decrease(request.Quantity);  // throws if insufficient

        var movement = StockMovement.Create(
            _tenant.TenantId, request.WarehouseId, request.ProductId,
            MovementType.Issue, request.Quantity, balance.Quantity,
            request.MovedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            reference:   request.Reference,
            notes:       request.Notes,
            batchNumber: request.BatchNumber);

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await LoadMovementDto(movement.Id);
    }

    public async Task<StockMovementDto> AdjustAsync(AdjustStockRequest request)
    {
        if (request.NewQuantity < 0)
            throw new ArgumentException("Adjusted quantity cannot be negative.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var balance = await GetOrCreateBalanceAsync(request.WarehouseId, request.ProductId);
        var diff    = request.NewQuantity - balance.Quantity;

        MovementType type;
        decimal qty;

        if (diff == 0)
            throw new InvalidOperationException("New quantity is the same as the current balance; no adjustment needed.");

        if (diff > 0) { type = MovementType.AdjustUp;   qty = diff;  balance.Increase(diff); }
        else          { type = MovementType.AdjustDown;  qty = -diff; balance.Decrease(-diff); }

        var movement = StockMovement.Create(
            _tenant.TenantId, request.WarehouseId, request.ProductId,
            type, qty, balance.Quantity,
            request.MovedAt?.ToUniversalTime() ?? DateTime.UtcNow,
            notes: request.Notes);

        _db.StockMovements.Add(movement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return await LoadMovementDto(movement.Id);
    }

    public async Task<(StockMovementDto Out, StockMovementDto In)> TransferAsync(TransferStockRequest request)
    {
        if (request.FromWarehouseId == request.ToWarehouseId)
            throw new ArgumentException("Source and destination warehouses must be different.");

        await using var tx = await _db.Database.BeginTransactionAsync();

        var pairId = Guid.NewGuid();   // links the two movement legs
        var movedAt = request.MovedAt?.ToUniversalTime() ?? DateTime.UtcNow;

        // Source: decrease
        var fromBalance = await GetOrCreateBalanceAsync(request.FromWarehouseId, request.ProductId);
        fromBalance.Decrease(request.Quantity);

        var outMovement = StockMovement.Create(
            _tenant.TenantId, request.FromWarehouseId, request.ProductId,
            MovementType.TransferOut, request.Quantity, fromBalance.Quantity,
            movedAt,
            reference:      request.Reference,
            notes:          request.Notes,
            transferPairId: pairId,
            batchNumber:    request.BatchNumber,
            expiryDate:     request.ExpiryDate);

        // Destination: increase
        var toBalance = await GetOrCreateBalanceAsync(request.ToWarehouseId, request.ProductId);
        toBalance.Increase(request.Quantity);

        var inMovement = StockMovement.Create(
            _tenant.TenantId, request.ToWarehouseId, request.ProductId,
            MovementType.TransferIn, request.Quantity, toBalance.Quantity,
            movedAt,
            reference:      request.Reference,
            notes:          request.Notes,
            transferPairId: pairId,
            batchNumber:    request.BatchNumber,
            expiryDate:     request.ExpiryDate);

        _db.StockMovements.AddRange(outMovement, inMovement);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return (await LoadMovementDto(outMovement.Id),
                await LoadMovementDto(inMovement.Id));
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    /// <summary>
    /// Returns the existing balance row or creates a new one (quantity = 0).
    /// Uses EF tracking so the caller can mutate it and SaveChanges will persist it.
    /// </summary>
    private async Task<StockBalance> GetOrCreateBalanceAsync(Guid warehouseId, Guid productId)
    {
        var balance = await _db.StockBalances
            .FirstOrDefaultAsync(b => b.WarehouseId == warehouseId && b.ProductId == productId);

        if (balance is not null) return balance;

        balance = StockBalance.CreateNew(_tenant.TenantId, warehouseId, productId);
        _db.StockBalances.Add(balance);
        return balance;
    }

    private async Task<StockMovementDto> LoadMovementDto(Guid movementId)
    {
        var m = await _db.StockMovements
            .Include(x => x.Warehouse)
            .Include(x => x.Product)
            .FirstAsync(x => x.Id == movementId);

        return MapMovement(m);
    }

    private static StockMovementDto MapMovement(StockMovement m) => new(
        m.Id,
        m.WarehouseId, m.Warehouse!.Name,
        m.ProductId, m.Product!.Code, m.Product.Name,
        m.Type,
        m.Quantity, m.BalanceAfter,
        m.Reference, m.Notes,
        m.BatchNumber, m.ExpiryDate,
        m.MovedAt, m.CreatedAt);
}
