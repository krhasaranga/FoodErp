using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Inventory;

public record WarehouseDto(
    Guid Id,
    string Name,
    string? Location,
    bool IsActive,
    DateTime CreatedAt);

public record CreateWarehouseRequest(
    string Name,
    string? Location);

public record UpdateWarehouseRequest(
    string Name,
    string? Location);

// ── Stock Balance ────────────────────────────────────────────────────────────

public record StockBalanceDto(
    Guid ProductId,
    string ProductCode,
    string ProductName,
    string UnitSymbol,
    Guid WarehouseId,
    string WarehouseName,
    decimal Quantity);

// ── Stock Movements ──────────────────────────────────────────────────────────

public record StockMovementDto(
    Guid Id,
    Guid WarehouseId,
    string WarehouseName,
    Guid ProductId,
    string ProductCode,
    string ProductName,
    MovementType Type,
    decimal Quantity,
    decimal BalanceAfter,
    string? Reference,
    string? Notes,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    DateTime MovedAt,
    DateTime CreatedAt);

// ── Stock Operation Requests ─────────────────────────────────────────────────

public record ReceiveStockRequest(
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    DateTime? MovedAt);       // defaults to UtcNow if null

public record IssueStockRequest(
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes,
    string? BatchNumber,
    DateTime? MovedAt);

public record AdjustStockRequest(
    Guid WarehouseId,
    Guid ProductId,
    decimal NewQuantity,      // the EXACT quantity it should be after adjustment
    string? Notes,
    DateTime? MovedAt);

public record TransferStockRequest(
    Guid FromWarehouseId,
    Guid ToWarehouseId,
    Guid ProductId,
    decimal Quantity,
    string? Reference,
    string? Notes,
    string? BatchNumber,
    DateOnly? ExpiryDate,
    DateTime? MovedAt);

public record OpeningStockRequest(
    Guid WarehouseId,
    Guid ProductId,
    decimal Quantity,
    string? Notes,
    string? BatchNumber,
    DateOnly? ExpiryDate);
