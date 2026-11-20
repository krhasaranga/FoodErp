using FoodErp.Application.DTOs.Invoicing;
using FoodErp.Domain.Entities;

namespace FoodErp.Application.Interfaces.Invoicing;

public interface IInvoiceService
{
    Task<IEnumerable<InvoiceListDto>> GetAllAsync(
        InvoiceStatus? status = null,
        Guid? customerId = null,
        bool overdueOnly = false);

    Task<InvoiceDto?> GetByIdAsync(Guid id);

    /// <summary>Create a blank invoice with manually entered lines.</summary>
    Task<InvoiceDto> CreateAsync(CreateInvoiceRequest request);

    /// <summary>
    /// Generate an invoice directly from a SalesOrder.
    /// Copies all order lines into invoice lines automatically.
    /// </summary>
    Task<InvoiceDto> GenerateFromSalesOrderAsync(GenerateInvoiceFromOrderRequest request);

    Task<InvoiceDto> UpdateAsync(Guid id, UpdateInvoiceRequest request);

    /// <summary>Locks the invoice and sends it — no further editing allowed.</summary>
    Task<InvoiceDto> IssueAsync(Guid id);

    Task<InvoiceDto> CancelAsync(Guid id);
}
