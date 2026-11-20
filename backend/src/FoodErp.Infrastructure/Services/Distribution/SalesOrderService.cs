using FoodErp.Application.DTOs.Distribution;
using FoodErp.Application.DTOs.Inventory;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Distribution;
using FoodErp.Application.Interfaces.Inventory;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Distribution;

public class SalesOrderService : ISalesOrderService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;
    private readonly IStockService _stock;

    public SalesOrderService(AppDbContext db, ITenantContext tenant, IStockService stock)
    {
        _db = db;
        _tenant = tenant;
        _stock = stock;
    }

    public async Task<IEnumerable<SalesOrderListDto>> GetAllAsync(
        SalesOrderStatus? status = null, Guid? customerId = null)
    {
        var query = _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .AsQueryable();

        if (status.HasValue)    query = query.Where(o => o.Status == status.Value);
        if (customerId.HasValue) query = query.Where(o => o.CustomerId == customerId.Value);

        var orders = await query.OrderByDescending(o => o.OrderDate).ToListAsync();

        return orders.Select(o => new SalesOrderListDto(
            o.Id, o.OrderNumber,
            o.Customer!.Name,
            o.Status,
            o.OrderDate,
            o.Lines.Sum(l => l.LineTotal)));
    }

    public async Task<SalesOrderDto?> GetByIdAsync(Guid id)
    {
        var order = await LoadFullOrderAsync(id);
        return order is null ? null : MapToDto(order);
    }

    public async Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request)
    {
        var orderNumber = await GenerateOrderNumberAsync();

        var order = SalesOrder.Create(
            orderNumber,
            request.CustomerId,
            request.WarehouseId,
            request.OrderDate,
            _tenant.TenantId,
            request.ExpectedDeliveryDate,
            request.DeliveryAddress,
            request.Notes);

        _db.SalesOrders.Add(order);
        await _db.SaveChangesAsync(); // get order Id

        // Add lines — snapshot the product's selling price if not provided
        foreach (var lineReq in request.Lines)
        {
            var product = await _db.Products.FindAsync(lineReq.ProductId)
                ?? throw new KeyNotFoundException($"Product {lineReq.ProductId} not found.");

            var unitPrice = lineReq.UnitPrice ?? product.SellingPrice;

            var line = SalesOrderLine.Create(
                order.Id,
                lineReq.ProductId,
                product.UnitId,       // use the product's default stock unit
                lineReq.Quantity,
                unitPrice,
                _tenant.TenantId,
                lineReq.DiscountPercent);

            _db.SalesOrderLines.Add(line);
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(order.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created sales order.");
    }

    public async Task<SalesOrderDto> UpdateAsync(Guid id, UpdateSalesOrderRequest request)
    {
        var order = await _db.SalesOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Sales order {id} not found.");

        order.UpdateDraftDetails(
            request.ExpectedDeliveryDate,
            request.DeliveryAddress,
            request.Notes);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated sales order.");
    }

    public async Task<SalesOrderDto> ConfirmAsync(Guid id)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Sales order {id} not found.");

        order.Confirm(); // throws if no lines
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve sales order.");
    }

    public async Task<SalesOrderDto> DispatchAsync(Guid id, DispatchOrderRequest request)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Lines)
            .FirstOrDefaultAsync(o => o.Id == id)
            ?? throw new KeyNotFoundException($"Sales order {id} not found.");

        // Status transition first (throws if wrong status)
        order.Dispatch();

        // Issue stock for each line from the order's warehouse
        foreach (var line in order.Lines)
        {
            await _stock.IssueAsync(new IssueStockRequest(
                WarehouseId: order.WarehouseId,
                ProductId:   line.ProductId,
                Quantity:    line.Quantity,
                Reference:   order.OrderNumber,
                Notes:       $"Sales order {order.OrderNumber} dispatched",
                BatchNumber: null,
                MovedAt:     DateTime.UtcNow));
        }

        if (request.Notes is not null)
            order.UpdateDraftDetails(null, null, request.Notes);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve dispatched sales order.");
    }

    public async Task<SalesOrderDto> MarkDeliveredAsync(Guid id)
    {
        var order = await _db.SalesOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Sales order {id} not found.");

        order.MarkDelivered();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve sales order.");
    }

    public async Task<SalesOrderDto> CancelAsync(Guid id)
    {
        var order = await _db.SalesOrders.FindAsync(id)
            ?? throw new KeyNotFoundException($"Sales order {id} not found.");

        order.Cancel();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve sales order.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GenerateOrderNumberAsync()
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.SalesOrders.CountAsync(o => o.CreatedAt.Year == year);
        return $"SO-{year}-{(count + 1):D4}";
    }

    private async Task<SalesOrder?> LoadFullOrderAsync(Guid id)
        => await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Warehouse)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(o => o.Id == id);

    private static SalesOrderDto MapToDto(SalesOrder o)
    {
        var lines    = o.Lines.ToList();
        var subTotal = lines.Sum(l => l.Quantity * l.UnitPrice);
        var discount = lines.Sum(l => l.Quantity * l.UnitPrice * l.DiscountPercent / 100m);
        var grand    = lines.Sum(l => l.LineTotal);

        return new SalesOrderDto(
            o.Id, o.OrderNumber,
            o.CustomerId, o.Customer!.Code, o.Customer.Name,
            o.WarehouseId, o.Warehouse!.Name,
            o.Status,
            o.OrderDate, o.ExpectedDeliveryDate,
            o.DispatchedAt, o.DeliveredAt,
            o.DeliveryAddress, o.Notes,
            Math.Round(subTotal, 4),
            Math.Round(discount, 4),
            Math.Round(grand, 4),
            o.CreatedAt,
            lines.Select(l => new SalesOrderLineDto(
                l.Id,
                l.ProductId, l.Product!.Code, l.Product.Name,
                l.UnitId, l.Unit!.Symbol,
                l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal)));
    }
}
