namespace FoodErp.Application.DTOs.Products;

public record CategoryDto(
    Guid Id,
    string Name,
    string? Description,
    Guid? ParentCategoryId,
    string? ParentCategoryName,
    bool IsActive,
    DateTime CreatedAt);

public record CreateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId);

public record UpdateCategoryRequest(
    string Name,
    string? Description,
    Guid? ParentCategoryId);