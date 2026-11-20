using FoodErp.Application.DTOs.Invoicing;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Invoicing;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Invoicing;

public class InvoiceService : IInvoiceService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public InvoiceService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<InvoiceListDto>> GetAllAsync(
        InvoiceStatus? status = null, Guid? customerId = null, bool overdueOnly = false)
    {
        var invoices = await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.Lines)
            .ToListAsync();

        // Apply in-memory filters (IsOverdue uses DateTime which can't translate to SQL easily)
        if (status.HasValue)    invoices = invoices.Where(i => i.Status == status.Value).ToList();
        if (customerId.HasValue) invoices = invoices.Where(i => i.CustomerId == customerId.Value).ToList();
        if (overdueOnly)         invoices = invoices.Where(i => i.IsOverdue).ToList();

        return invoices
            .OrderByDescending(i => i.InvoiceDate)
            .Select(i => new InvoiceListDto(
                i.Id, i.InvoiceNumber,
                i.Customer!.Name,
                i.Status,
                i.InvoiceDate, i.DueDate, i.IsOverdue,
                i.TotalAmount, i.OutstandingAmount));
    }

    public async Task<InvoiceDto?> GetByIdAsync(Guid id)
    {
        var invoice = await LoadFullInvoiceAsync(id);
        return invoice is null ? null : MapToDto(invoice);
    }

    public async Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request)
    {
        var invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = Invoice.Create(
            invoiceNumber,
            request.CustomerId,
            request.InvoiceDate,
            request.DueDate,
            _tenant.TenantId,
            notes: request.Notes);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        foreach (var lineReq in request.Lines)
        {
            var line = InvoiceLine.Create(
                invoice.Id,
                lineReq.Description,
                lineReq.Quantity,
                lineReq.UnitPrice,
                _tenant.TenantId,
                lineReq.ProductId,
                lineReq.DiscountPercent);
            _db.InvoiceLines.Add(line);
        }

        await _db.SaveChangesAsync();

        // Reload with lines to calculate total
        var full = await LoadFullInvoiceAsync(invoice.Id)!;
        full!.RecalculateTotal();
        await _db.SaveChangesAsync();

        return MapToDto(full);
    }

    public async Task<InvoiceDto> GenerateFromSalesOrderAsync(GenerateInvoiceFromOrderRequest request)
    {
        var order = await _db.SalesOrders
            .Include(o => o.Lines)
                .ThenInclude(l => l.Product)
            .Include(o => o.Lines)
                .ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(o => o.Id == request.SalesOrderId)
            ?? throw new KeyNotFoundException($"Sales order {request.SalesOrderId} not found.");

        if (order.Status == SalesOrderStatus.Cancelled)
            throw new InvalidOperationException("Cannot invoice a cancelled sales order.");

        // Check if an invoice already exists for this order
        var existingInvoice = await _db.Invoices
            .AnyAsync(i => i.SalesOrderId == request.SalesOrderId
                        && i.Status != InvoiceStatus.Cancelled);

        if (existingInvoice)
            throw new InvalidOperationException("An invoice already exists for this sales order.");

        var invoiceNumber = await GenerateInvoiceNumberAsync();

        var invoice = Invoice.Create(
            invoiceNumber,
            order.CustomerId,
            DateTime.UtcNow,
            request.DueDate,
            _tenant.TenantId,
            salesOrderId: order.Id,
            notes: request.Notes);

        _db.Invoices.Add(invoice);
        await _db.SaveChangesAsync();

        // Copy sales order lines into invoice lines
        foreach (var orderLine in order.Lines)
        {
            var description = orderLine.Product?.Name ?? "Product";
            var line = InvoiceLine.Create(
                invoice.Id,
                description,
                orderLine.Quantity,
                orderLine.UnitPrice,
                _tenant.TenantId,
                orderLine.ProductId,
                orderLine.DiscountPercent);
            _db.InvoiceLines.Add(line);
        }

        await _db.SaveChangesAsync();

        var full = await LoadFullInvoiceAsync(invoice.Id)!;
        full!.RecalculateTotal();
        await _db.SaveChangesAsync();

        return MapToDto(full);
    }

    public async Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceRequest request)
    {
        var invoice = await _db.Invoices.FindAsync(id)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        invoice.UpdateDraftDetails(request.DueDate, request.Notes);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated invoice.");
    }

    public async Task<InvoiceDto> IssueAsync(Guid id)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Lines)
            .FirstOrDefaultAsync(i => i.Id == id)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        invoice.RecalculateTotal(); // ensure total is current
        invoice.Issue();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve issued invoice.");
    }

    public async Task<InvoiceDto> CancelAsync(Guid id)
    {
        var invoice = await _db.Invoices.FindAsync(id)
            ?? throw new KeyNotFoundException($"Invoice {id} not found.");

        invoice.Cancel();
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve cancelled invoice.");
    }

    // ── Helpers ───────────────────────────────────────────────────────────────

    private async Task<string> GenerateInvoiceNumberAsync()
    {
        var year  = DateTime.UtcNow.Year;
        var count = await _db.Invoices.CountAsync(i => i.CreatedAt.Year == year);
        return $"INV-{year}-{(count + 1):D4}";
    }

    private async Task<Invoice?> LoadFullInvoiceAsync(Guid id)
        => await _db.Invoices
            .Include(i => i.Customer)
            .Include(i => i.SalesOrder)
            .Include(i => i.Lines)
                .ThenInclude(l => l.Product)
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == id);

    private static InvoiceDto MapToDto(Invoice i) => new(
        i.Id, i.InvoiceNumber,
        i.CustomerId, i.Customer!.Code, i.Customer.Name,
        i.SalesOrderId, i.SalesOrder?.OrderNumber,
        i.Status,
        i.InvoiceDate, i.DueDate, i.IsOverdue,
        i.Notes,
        i.TotalAmount, i.PaidAmount, i.OutstandingAmount,
        i.CreatedAt,
        i.Lines.Select(l => new InvoiceLineDto(
            l.Id, l.ProductId, l.Description,
            l.Quantity, l.UnitPrice, l.DiscountPercent, l.LineTotal)),
        i.Payments.OrderByDescending(p => p.PaymentDate).Select(p => new PaymentDto(
            p.Id, p.InvoiceId, i.InvoiceNumber,
            p.Amount, p.Method, p.PaymentDate,
            p.Reference, p.Notes, p.CreatedAt)));
}
