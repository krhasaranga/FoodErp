using FoodErp.Application.DTOs.Products;

namespace FoodErp.Application.Interfaces.Products;

public interface IProductService
{
    Task<IEnumerable<ProductDto>> GetAllAsync();
    Task<ProductDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId);
    Task<ProductDto> CreateAsync(CreateProductRequest request);
    Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request);
    Task UpdatePricesAsync(Guid id, UpdateProductPricesRequest request);
    Task DeactivateAsync(Guid id);
}