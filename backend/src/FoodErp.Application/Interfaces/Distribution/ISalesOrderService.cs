using FoodErp.Application.DTOs.Distribution;
using FoodErp.Domain.Entities;

namespace FoodErp.Application.Interfaces.Distribution;

public interface ISalesOrderService
{
    Task<IEnumerable<SalesOrderListDto>> GetAllAsync(SalesOrderStatus? status = null, Guid? customerId = null);
    Task<SalesOrderDto?> GetByIdAsync(Guid id);
    Task<SalesOrderDto> CreateAsync(CreateSalesOrderRequest request);
    Task<SalesOrderDto> UpdateAsync(Guid id, UpdateSalesOrderRequest request);

    /// <summary>Locks the order — no further edits allowed.</summary>
    Task<SalesOrderDto> ConfirmAsync(Guid id);

    /// <summary>Issues stock from warehouse and marks order Dispatched.</summary>
    Task<SalesOrderDto> DispatchAsync(Guid id, DispatchOrderRequest request);

    /// <summary>Marks the order as received by the customer.</summary>
    Task<SalesOrderDto> MarkDeliveredAsync(Guid id);

    Task<SalesOrderDto> CancelAsync(Guid id);
}
