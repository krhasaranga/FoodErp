using FoodErp.Application.DTOs.Products;

namespace FoodErp.Application.Interfaces.Products;

public interface IUnitService
{
    Task<IEnumerable<UnitDto>> GetAllAsync();
    Task<UnitDto?> GetByIdAsync(Guid id);
    Task<UnitDto> CreateAsync(CreateUnitRequest request);
    Task<IEnumerable<UnitConversionDto>> GetConversionsAsync();
    Task<UnitConversionDto> CreateConversionAsync(CreateUnitConversionRequest request);
}