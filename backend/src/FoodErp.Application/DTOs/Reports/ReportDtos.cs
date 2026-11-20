using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Reports;

// ── Dashboard ─────────────────────────────────────────────────────────────────

public record DashboardDto(
    // Inventory
    int TotalProducts,
    int LowStockProducts,          // products below reorder level

    // Today's activity
    decimal TodaySalesAmount,
    int TodayOrdersCount,
    int ActiveProductionOrders,

    // Financials
    decimal TotalOutstandingReceivables,
    int OverdueInvoicesCount,
    decimal OverdueInvoicesAmount,

    // Operations
    int PendingDispatchOrders,     // confirmed but not yet dispatched
    int EmployeesPresentToday);

// ── Inventory Report ──────────────────────────────────────────────────────────

public record StockSummaryDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string CategoryName,
    string UnitSymbol,
    decimal TotalQuantity,         // across all warehouses
    decimal ReorderLevel,
    bool IsBelowReorderLevel);

public record StockMovementReportDto(
    DateTime MovedAt,
    string WarehouseName,
    string ProductCode,
    string ProductName,
    MovementType Type,
    decimal Quantity,
    decimal BalanceAfter,
    string? Reference,
    string? BatchNumber);

// ── Sales Report ──────────────────────────────────────────────────────────────

public record SalesReportItemDto(
    string OrderNumber,
    string CustomerName,
    DateTime OrderDate,
    SalesOrderStatus Status,
    decimal GrandTotal);

public record SalesByCustomerDto(
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    int OrderCount,
    decimal TotalSales);

// ── Production Report ─────────────────────────────────────────────────────────

public record ProductionReportItemDto(
    string OrderNumber,
    string RecipeName,
    string OutputProductName,
    decimal PlannedBatches,
    decimal? ActualBatches,
    ProductionOrderStatus Status,
    DateTime PlannedDate,
    DateTime? CompletedAt);

// ── Receivables Report ────────────────────────────────────────────────────────

public record ReceivablesByCustomerDto(
    Guid CustomerId,
    string CustomerCode,
    string CustomerName,
    int TotalInvoices,
    decimal TotalInvoiced,
    decimal TotalPaid,
    decimal Outstanding,
    decimal OverdueAmount);

// ── Payment Report ────────────────────────────────────────────────────────────

public record PaymentReportItemDto(
    DateTime PaymentDate,
    string InvoiceNumber,
    string CustomerName,
    PaymentMethod Method,
    decimal Amount,
    string? Reference);
