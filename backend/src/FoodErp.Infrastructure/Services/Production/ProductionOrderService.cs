using FoodErp.Application.DTOs.Inventory;
using FoodErp.Application.DTOs.Production;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Inventory;
using FoodErp.Application.Interfaces.Production;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Production;

public class ProductionOrderService : IProductionOrderService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStockService _stock;

    public ProductionOrderService(AppDbContext db, ITenantContext tenant, IStockService stock)
    {
        _db = db;
        _tenant = tenant;
        _stock = stock;
    }

    public async Task<IEnumerable<ProductionOrderListDto>> GetAllAsync(ProductionOrderStatus? status = null)
    {
        var query = _db.ProductionOrders
            .Include(o => o.Recipe)
                .ThenInclude(r => r!.OutputProduct)
            .AsQueryable();

        if (status.HasValue)
            query = query.Where(o => o.Status == status.Value);

        var orders = await query.OrderByDescending(o => o.PlannedDate).ToListAsync();

        return orders.Select(o => new ProductionOrderListDto(
            o.Id, o.OrderNumber,
            o.Recipe!.Name,
            o.Recipe.OutputProduct!.Name,
            o.PlannedBatches,
            o.Status,
            o.PlannedDate));
    }

    public async Task<ProductionOrderDto?> GetByIdAsync(Guid id)
    {
        var order = await LoadFullOrderAsync(id);
        return order is null ? null : MapToDto(order);
    }

    public async Task<ProductionOrderDto> CreateAsync(CreateProductionOrderRequest request)
    {
        // Load the recipe to build planned lines
        var recipe = await _db.Recipes
            .Include(r => r.Lines)
            .FirstOrDefaultAsync(r => r.Id == request.RecipeId)
            ?? throw new KeyNotFoundException($"Recipe {request.RecipeId} not found.");

        if (!recipe.IsActive)
            throw new InvalidOperationException("Cannot create a production order for an inactive recipe.");

        // Generate order number: PRD-YYYY-NNN
        var orderNumber = await GenerateOrderNumberAsync();

        var order = ProductionOrder.Create(
            orderNumber,
            request.RecipeId,
            request.InputWarehouseId,
            request.OutputWarehouseId,
            request.PlannedBatches,
            request.PlannedDate,
            _tenant.TenantId,
            request.Notes);

        _db.ProductionOrders.Add(order);
        await _db.SaveChangesAsync(); // get order Id

        // Create planned lines from recipe (scaled by PlannedBatches)
        foreach (var recipeLine in recipe.Lines)
        {
            var plannedQty = recipeLine.Quantity * request.PlannedBatches;
            var line = ProductionOrderLine.Create(
                order.Id,
                recipeLine.IngredientProductId,
                plannedQty,
                recipeLine.UnitId,
                _tenant.TenantId);
            _db.ProductionOrderLines.Add(line);
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created production order.");
    }

    public async Task<ProductionOrderDto> StartAsync(Guid id)
    {
        var order = await _db.ProductionOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production order {id} not found.");

        order.Start();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve production order.");
    }

    public async Task<ProductionOrderDto> CompleteAsync(Guid id, CompleteProductionOrderRequest request)
    {
        var order = await _db.ProductionOrders
            .Include(o => o.Recipe)
                .ThenInclude(r => r!.OutputProduct)
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Production order {id} not found.");

        // Validate status transition
        order.Complete(request.ActualBatches);

        // --- Issue ingredients from InputWarehouse ---
        foreach (var line in order.Lines)
        {
            var actualQty = request.ActualQuantities != null
                && request.ActualQuantities.TryGetValue(line.IngredientProductId, out var aq)
                    ? aq
                    : line.PlannedQuantity;

            line.RecordActual(actualQty);

            if (actualQty > 0)
            {
                await _stock.IssueAsync(new IssueStockRequest(
                    WarehouseId: order.InputWarehouseId,
                    ProductId:   line.IngredientProductId,
                    Quantity:    actualQty,
                    Reference:   order.OrderNumber,
                    Notes:       $"Production order {order.OrderNumber}",
                    BatchNumber: null,
                    MovedAt:     DateTime.UtcNow));
            }
        }

        // --- Receive finished goods into OutputWarehouse ---
        var actualOutputQty = order.Recipe!.OutputQuantity * request.ActualBatches;

        await _stock.ReceiveAsync(new ReceiveStockRequest(
            WarehouseId: order.OutputWarehouseId,
            ProductId:   order.Recipe.OutputProductId,
            Quantity:    actualOutputQty,
            Reference:   order.OrderNumber,
            Notes:       $"Production order {order.OrderNumber} completed",
            BatchNumber: null,
            ExpiryDate:  null,
            MovedAt:     DateTime.UtcNow));

        if (request.Notes is not null)
            order.UpdateNotes(request.Notes);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve completed production order.");
    }

    public async Task<ProductionOrderDto> CancelAsync(Guid id)
    {
        var order = await _db.ProductionOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Production order {id} not found.");

        order.Cancel();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve production order.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GenerateOrderNumberAsync()
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.ProductionOrders
            .CountAsync(o => o.CreatedAt.Year == year);
        return $"PRD-{year}-{(count + 1):D4}";
    }

    private async Task<ProductionOrder?> LoadFullOrderAsync(Guid id)
        => await _db.ProductionOrders
            .Include(o => o.Recipe)
                .ThenInclude(r => r!.OutputProduct)
            .Include(o => o.Recipe)
                .ThenInclude(r => r!.OutputUnit)
            .Include(o => o.InputWarehouse)
            .Include(o => o.OutputWarehouse)
            .Include(o => o.Lines)
                .ThenInclude(l => l.IngredientProduct)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(o => o.Id == id);

    private static ProductionOrderDto MapToDto(ProductionOrder o)
    {
        var recipe = o.Recipe!;
        var plannedOutput = recipe.OutputQuantity * o.PlannedBatches;
        var actualOutput  = o.ActualBatches.HasValue
            ? recipe.OutputQuantity * o.ActualBatches.Value
            : (decimal?)null;

        return new ProductionOrderDto(
            o.Id, o.OrderNumber,
            o.RecipeId, recipe.Name,
            recipe.OutputProduct!.Name,
            o.InputWarehouseId,  o.InputWarehouse!.Name,
            o.OutputWarehouseId, o.OutputWarehouse!.Name,
            o.PlannedBatches, o.ActualBatches,
            plannedOutput, actualOutput,
            o.Status,
            o.PlannedDate, o.StartedAt, o.CompletedAt,
            o.Notes, o.CreatedAt,
            o.Lines.Select(l => new ProductionOrderLineDto(
                l.Id,
                l.IngredientProductId,
                l.IngredientProduct!.Code,
                l.IngredientProduct.Name,
                l.PlannedQuantity,
                l.ActualQuantity,
                l.UnitId,
                l.Unit!.Symbol)));
    }
}
