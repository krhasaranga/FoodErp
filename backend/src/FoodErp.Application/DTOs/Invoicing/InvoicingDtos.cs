using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Invoicing;

// ── Invoice ───────────────────────────────────────────────────────────────────

public record InvoiceLineDto(
    Guid Id,
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal);

public record InvoiceDto(
    Guid Id,
    string InvoiceNumber,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    Guid? SalesOrderId,
    string? SalesOrderNumber,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime DueDate,
    bool IsOverdue,
    string? Notes,
    decimal TotalAmount,
    decimal PaidAmount,
    decimal OutstandingAmount,
    DateTime CreatedAt,
    IEnumerable<InvoiceLineDto> Lines,
    IEnumerable<PaymentDto> Payments);

public record InvoiceListDto(
    Guid Id,
    string InvoiceNumber,
    string CustomerName,
    InvoiceStatus Status,
    DateTime InvoiceDate,
    DateTime DueDate,
    bool IsOverdue,
    decimal TotalAmount,
    decimal OutstandingAmount);

public record CreateInvoiceRequest(
    Guid CustomerId,
    DateTime InvoiceDate,
    DateTime DueDate,
    string? Notes,
    IEnumerable<CreateInvoiceLineRequest> Lines);

public record CreateInvoiceLineRequest(
    Guid? ProductId,
    string Description,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent = 0);

public record UpdateInvoiceRequest(
    DateTime DueDate,
    string? Notes);

public record GenerateInvoiceFromOrderRequest(
    Guid SalesOrderId,
    DateTime DueDate,
    string? Notes);

// ── Payment ───────────────────────────────────────────────────────────────────

public record PaymentDto(
    Guid Id,
    Guid InvoiceId,
    string InvoiceNumber,
    decimal Amount,
    PaymentMethod Method,
    DateTime PaymentDate,
    string? Reference,
    string? Notes,
    DateTime CreatedAt);

public record RecordPaymentRequest(
    Guid InvoiceId,
    decimal Amount,
    PaymentMethod Method,
    DateTime PaymentDate,
    string? Reference,
    string? Notes);
