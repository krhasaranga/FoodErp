using FoodErp.Domain.Entities;

namespace FoodErp.Application.DTOs.Products;

public record ProductDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    ProductType Type,
    Guid CategoryId,
    string CategoryName,
    Guid UnitId,
    string UnitSymbol,
    decimal CostPrice,
    decimal SellingPrice,
    decimal ReorderLevel,
    bool TrackExpiry,
    bool IsActive,
    DateTime CreatedAt);

public record CreateProductRequest(
    string Code,
    string Name,
    string? Description,
    ProductType Type,
    Guid CategoryId,
    Guid UnitId,
    decimal CostPrice,
    decimal SellingPrice,
    decimal ReorderLevel = 0,
    bool TrackExpiry = false);

public record UpdateProductRequest(
    string Code,
    string Name,
    string? Description,
    Guid CategoryId,
    Guid UnitId,
    decimal ReorderLevel,
    bool TrackExpiry);

public record UpdateProductPricesRequest(
    decimal CostPrice,
    decimal SellingPrice);