using FoodErp.Application.DTOs.Products;

namespace FoodErp.Application.Interfaces.Products;

public interface IProductCategoryService
{
    Task<IEnumerable<CategoryDto>> GetAllAsync();
    Task<CategoryDto?> GetByIdAsync(Guid id);
    Task<CategoryDto> CreateAsync(CreateCategoryRequest request);
    Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request);
    Task DeactivateAsync(Guid id);
}