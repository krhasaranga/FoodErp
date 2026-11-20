using FoodErp.Application.DTOs.Inventory;

namespace FoodErp.Application.Interfaces.Inventory;

public interface IWarehouseService
{
    Task<IEnumerable<WarehouseDto>> GetAllAsync();
    Task<WarehouseDto?> GetByIdAsync(Guid id);
    Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request);
    Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseRequest request);
    Task DeactivateAsync(Guid id);
}
