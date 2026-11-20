using FoodErp.Application.DTOs.Production;
using FoodErp.Domain.Entities;

namespace FoodErp.Application.Interfaces.Production;

public interface IProductionOrderService
{
    Task<IEnumerable<ProductionOrderListDto>> GetAllAsync(ProductionOrderStatus? status = null);
    Task<ProductionOrderDto?> GetByIdAsync(Guid id);
    Task<ProductionOrderDto> CreateAsync(CreateProductionOrderRequest request);
    Task<ProductionOrderDto> StartAsync(Guid id);
    Task<ProductionOrderDto> CompleteAsync(Guid id, CompleteProductionOrderRequest request);
    Task<ProductionOrderDto> CancelAsync(Guid id);
}
