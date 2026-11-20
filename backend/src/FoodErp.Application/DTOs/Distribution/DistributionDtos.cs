using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Distribution;

// ── Customer ─────────────────────────────────────────────────────────────────

public record CustomerDto(
    Guid Id,
    string Code,
    string Name,
    CustomerType Type,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit,
    bool IsActive,
    DateTime CreatedAt);

public record CustomerListDto(
    Guid Id,
    string Code,
    string Name,
    CustomerType Type,
    string? Phone,
    bool IsActive);

public record CreateCustomerRequest(
    string Code,
    string Name,
    CustomerType Type,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit);

public record UpdateCustomerRequest(
    string Name,
    CustomerType Type,
    string? ContactPerson,
    string? Phone,
    string? Email,
    string? Address,
    decimal? CreditLimit);

// ── Sales Order ───────────────────────────────────────────────────────────────

public record SalesOrderLineDto(
    Guid Id,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    Guid UnitId,
    string UnitSymbol,
    decimal Quantity,
    decimal UnitPrice,
    decimal DiscountPercent,
    decimal LineTotal);

public record SalesOrderDto(
    Guid Id,
    string OrderNumber,
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    Guid WarehouseId,
    string WarehouseName,
    SalesOrderStatus Status,
    DateTime OrderDate,
    DateTime? ExpectedDeliveryDate,
    DateTime? DispatchedAt,
    DateTime? DeliveredAt,
    string? DeliveryAddress,
    string? Notes,
    decimal SubTotal,
    decimal TotalDiscount,
    decimal GrandTotal,
    DateTime CreatedAt,
    IEnumerable<SalesOrderLineDto> Lines);

public record SalesOrderListDto(
    Guid Id,
    string OrderNumber,
    string CustomerName,
    SalesOrderStatus Status,
    DateTime OrderDate,
    decimal GrandTotal);

public record CreateSalesOrderRequest(
    Guid CustomerId,
    Guid WarehouseId,
    DateTime OrderDate,
    DateTime? ExpectedDeliveryDate,
    string? DeliveryAddress,
    string? Notes,
    IEnumerable<CreateSalesOrderLineRequest> Lines);

public record CreateSalesOrderLineRequest(
    Guid ProductId,
    decimal Quantity,
    /// <summary>If null, the product's current SellingPrice is used.</summary>
    decimal? UnitPrice,
    decimal DiscountPercent = 0);

public record UpdateSalesOrderRequest(
    DateTime? ExpectedDeliveryDate,
    string? DeliveryAddress,
    string? Notes);

public record DispatchOrderRequest(
    string? Notes);
