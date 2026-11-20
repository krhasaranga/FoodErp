using FoodErp.Application.DTOs.Reports;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Reports;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Reports;

public class ReportService : IReportService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ReportService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<DashboardDto> GetDashboardAsync()
    {
        var today    = DateTime.UtcNow.Date;
        var todayEnd = today.AddDays(1);
        var todayDate = DateOnly.FromDateTime(DateTime.UtcNow);

        // Products & Stock
        var totalProducts = await _db.Products.CountAsync(p => p.IsActive);

        var balances = await _db.StockBalances
            .Include(b => b.Product)
            .ToListAsync();

        var lowStock = balances
            .GroupBy(b => b.ProductId)
            .Count(g =>
            {
                var product = g.First().Product!;
                var total   = g.Sum(b => b.Quantity);
                return product.ReorderLevel > 0 && total <= product.ReorderLevel;
            });

        // Today's sales
        var todayOrders = await _db.SalesOrders
            .Include(o => o.Lines)
            .Where(o => o.OrderDate >= today && o.OrderDate < todayEnd
                     && o.Status != SalesOrderStatus.Cancelled)
            .ToListAsync();

        var todaySales  = todayOrders.Sum(o => o.Lines.Sum(l => l.LineTotal));

        // Active production
        var activeProduction = await _db.ProductionOrders
            .CountAsync(o => o.Status == ProductionOrderStatus.InProgress
                          || o.Status == ProductionOrderStatus.Draft);

        // Financials
        var invoices = await _db.Invoices
            .Where(i => i.Status != InvoiceStatus.Cancelled && i.Status != InvoiceStatus.Paid)
            .ToListAsync();

        var totalOutstanding = invoices.Sum(i => i.OutstandingAmount);
        var overdueInvoices  = invoices.Where(i => i.IsOverdue).ToList();

        // Operations
        var pendingDispatch = await _db.SalesOrders
            .CountAsync(o => o.Status == SalesOrderStatus.Confirmed);

        // Attendance today
        var presentToday = await _db.Attendance
            .CountAsync(a => a.Date == todayDate
                          && (a.Status == AttendanceStatus.Present
                           || a.Status == AttendanceStatus.HalfDay));

        return new DashboardDto(
            totalProducts, lowStock,
            todaySales, todayOrders.Count,
            activeProduction,
            totalOutstanding,
            overdueInvoices.Count,
            overdueInvoices.Sum(i => i.OutstandingAmount),
            pendingDispatch,
            presentToday);
    }

    public async Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync(bool lowStockOnly = false)
    {
        var balances = await _db.StockBalances
            .Include(b => b.Product)
                .ThenInclude(p => p!.Category)
            .Include(b => b.Product)
                .ThenInclude(p => p!.Unit)
            .ToListAsync();

        var grouped = balances
            .GroupBy(b => b.ProductId)
            .Select(g =>
            {
                var product = g.First().Product!;
                var total   = g.Sum(b => b.Quantity);
                return new StockSummaryDto(
                    product.Id,
                    product.Code,
                    product.Name,
                    product.Category?.Name ?? "-",
                    product.Unit?.Symbol ?? "-",
                    total,
                    product.ReorderLevel,
                    product.ReorderLevel > 0 && total <= product.ReorderLevel);
            })
            .OrderBy(s => s.ProductName);

        return lowStockOnly
            ? grouped.Where(s => s.IsBelowReorderLevel)
            : grouped;
    }

    public async Task<IEnumerable<StockMovementReportDto>> GetStockMovementsAsync(
        DateTime from, DateTime to, Guid? warehouseId = null, Guid? productId = null)
    {
        var query = _db.StockMovements
            .Include(m => m.Warehouse)
            .Include(m => m.Product)
            .Where(m => m.MovedAt >= from.ToUniversalTime() && m.MovedAt <= to.ToUniversalTime())
            .AsQueryable();

        if (warehouseId.HasValue) query = query.Where(m => m.WarehouseId == warehouseId.Value);
        if (productId.HasValue)   query = query.Where(m => m.ProductId   == productId.Value);

        var movements = await query.OrderByDescending(m => m.MovedAt).ToListAsync();

        return movements.Select(m => new StockMovementReportDto(
            m.MovedAt,
            m.Warehouse!.Name,
            m.Product!.Code,
            m.Product.Name,
            m.Type,
            m.Quantity,
            m.BalanceAfter,
            m.Reference,
            m.BatchNumber));
    }

    public async Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to)
    {
        var orders = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Where(o => o.OrderDate >= from.ToUniversalTime()
                     && o.OrderDate <= to.ToUniversalTime()
                     && o.Status != SalesOrderStatus.Cancelled)
            .OrderByDescending(o => o.OrderDate)
            .ToListAsync();

        return orders.Select(o => new SalesReportItemDto(
            o.OrderNumber,
            o.Customer!.Name,
            o.OrderDate,
            o.Status,
            o.Lines.Sum(l => l.LineTotal)));
    }

    public async Task<IEnumerable<SalesByCustomerDto>> GetSalesByCustomerAsync(DateTime from, DateTime to)
    {
        var orders = await _db.SalesOrders
            .Include(o => o.Customer)
            .Include(o => o.Lines)
            .Where(o => o.OrderDate >= from.ToUniversalTime()
                     && o.OrderDate <= to.ToUniversalTime()
                     && o.Status != SalesOrderStatus.Cancelled)
            .ToListAsync();

        return orders
            .GroupBy(o => o.CustomerId)
            .Select(g =>
            {
                var customer = g.First().Customer!;
                return new SalesByCustomerDto(
                    customer.Id, customer.Code, customer.Name,
                    g.Count(),
                    g.Sum(o => o.Lines.Sum(l => l.LineTotal)));
            })
            .OrderByDescending(s => s.TotalSales);
    }

    public async Task<IEnumerable<ProductionReportItemDto>> GetProductionReportAsync(
        DateTime from, DateTime to)
    {
        var orders = await _db.ProductionOrders
            .Include(o => o.Recipe)
                .ThenInclude(r => r!.OutputProduct)
            .Where(o => o.PlannedDate >= from.ToUniversalTime()
                     && o.PlannedDate <= to.ToUniversalTime())
            .OrderByDescending(o => o.PlannedDate)
            .ToListAsync();

        return orders.Select(o => new ProductionReportItemDto(
            o.OrderNumber,
            o.Recipe!.Name,
            o.Recipe.OutputProduct!.Name,
            o.PlannedBatches,
            o.ActualBatches,
            o.Status,
            o.PlannedDate,
            o.CompletedAt));
    }

    public async Task<IEnumerable<ReceivablesByCustomerDto>> GetReceivablesReportAsync()
    {
        var invoices = await _db.Invoices
            .Include(i => i.Customer)
            .Where(i => i.Status != InvoiceStatus.Cancelled)
            .ToListAsync();

        return invoices
            .GroupBy(i => i.CustomerId)
            .Select(g =>
            {
                var customer      = g.First().Customer!;
                var outstanding   = g.Where(i => i.Status != InvoiceStatus.Paid);
                var overdueAmount = outstanding
                    .Where(i => i.IsOverdue)
                    .Sum(i => i.OutstandingAmount);

                return new ReceivablesByCustomerDto(
                    customer.Id, customer.Code, customer.Name,
                    g.Count(),
                    g.Sum(i => i.TotalAmount),
                    g.Sum(i => i.PaidAmount),
                    outstanding.Sum(i => i.OutstandingAmount),
                    overdueAmount);
            })
            .Where(r => r.Outstanding > 0)
            .OrderByDescending(r => r.Outstanding);
    }

    public async Task<IEnumerable<PaymentReportItemDto>> GetPaymentHistoryAsync(
        DateTime from, DateTime to)
    {
        var payments = await _db.Payments
            .Include(p => p.Invoice)
                .ThenInclude(i => i!.Customer)
            .Where(p => p.PaymentDate >= from.ToUniversalTime()
                     && p.PaymentDate <= to.ToUniversalTime())
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(p => new PaymentReportItemDto(
            p.PaymentDate,
            p.Invoice!.InvoiceNumber,
            p.Invoice.Customer!.Name,
            p.Method,
            p.Amount,
            p.Reference));
    }
}
