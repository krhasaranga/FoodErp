using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Production;

// ── Recipe ───────────────────────────────────────────────────────────────────

public record RecipeLineDto(
    Guid Id,
    Guid IngredientProductId,
    string IngredientProductCode,
    string IngredientProductName,
    decimal Quantity,
    Guid UnitId,
    string UnitSymbol,
    string? Notes);

public record RecipeDto(
    Guid Id,
    string Name,
    string? Description,
    Guid OutputProductId,
    string OutputProductCode,
    string OutputProductName,
    decimal OutputQuantity,
    Guid OutputUnitId,
    string OutputUnitSymbol,
    bool IsActive,
    DateTime CreatedAt,
    IEnumerable<RecipeLineDto> Lines);

public record RecipeListDto(
    Guid Id,
    string Name,
    string OutputProductName,
    decimal OutputQuantity,
    string OutputUnitSymbol,
    bool IsActive,
    int LineCount);

public record CreateRecipeRequest(
    string Name,
    string? Description,
    Guid OutputProductId,
    decimal OutputQuantity,
    Guid OutputUnitId,
    IEnumerable<CreateRecipeLineRequest> Lines);

public record CreateRecipeLineRequest(
    Guid IngredientProductId,
    decimal Quantity,
    Guid UnitId,
    string? Notes);

public record UpdateRecipeRequest(
    string Name,
    string? Description,
    decimal OutputQuantity,
    Guid OutputUnitId);

public record UpsertRecipeLineRequest(
    Guid IngredientProductId,
    decimal Quantity,
    Guid UnitId,
    string? Notes);

// ── Production Order ─────────────────────────────────────────────────────────

public record ProductionOrderLineDto(
    Guid Id,
    Guid IngredientProductId,
    string IngredientProductCode,
    string IngredientProductName,
    decimal PlannedQuantity,
    decimal? ActualQuantity,
    Guid UnitId,
    string UnitSymbol);

public record ProductionOrderDto(
    Guid Id,
    string OrderNumber,
    Guid RecipeId,
    string RecipeName,
    string OutputProductName,
    Guid InputWarehouseId,
    string InputWarehouseName,
    Guid OutputWarehouseId,
    string OutputWarehouseName,
    decimal PlannedBatches,
    decimal? ActualBatches,
    decimal PlannedOutputQuantity,
    decimal? ActualOutputQuantity,
    ProductionOrderStatus Status,
    DateTime PlannedDate,
    DateTime? StartedAt,
    DateTime? CompletedAt,
    string? Notes,
    DateTime CreatedAt,
    IEnumerable<ProductionOrderLineDto> Lines);

public record ProductionOrderListDto(
    Guid Id,
    string OrderNumber,
    string RecipeName,
    string OutputProductName,
    decimal PlannedBatches,
    ProductionOrderStatus Status,
    DateTime PlannedDate);

public record CreateProductionOrderRequest(
    Guid RecipeId,
    Guid InputWarehouseId,
    Guid OutputWarehouseId,
    decimal PlannedBatches,
    DateTime PlannedDate,
    string? Notes);

public record CompleteProductionOrderRequest(
    decimal ActualBatches,
    /// <summary>
    /// Actual quantities per ingredient. Key = IngredientProductId.
    /// If omitted for a line, the planned quantity is used.
    /// </summary>
    Dictionary<Guid, decimal>? ActualQuantities,
    string? Notes);
