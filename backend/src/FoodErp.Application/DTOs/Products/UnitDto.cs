namespace FoodErp.Application.DTOs.Products;

public record UnitDto(
    Guid Id,
    string Name,
    string Symbol,
    bool IsBaseUnit,
    bool IsActive);

public record CreateUnitRequest(
    string Name,
    string Symbol,
    bool IsBaseUnit = false);

public record CreateUnitConversionRequest(
    Guid FromUnitId,
    Guid ToUnitId,
    decimal Factor);

public record UnitConversionDto(
    Guid Id,
    Guid FromUnitId,
    string FromUnitSymbol,
    Guid ToUnitId,
    string ToUnitSymbol,
    decimal Factor);