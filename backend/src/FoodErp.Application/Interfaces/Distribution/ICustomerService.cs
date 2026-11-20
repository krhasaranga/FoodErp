using FoodErp.Application.DTOs.Distribution;

namespace FoodErp.Application.Interfaces.Distribution;

public interface ICustomerService
{
    Task<IEnumerable<CustomerListDto>> GetAllAsync();
    Task<CustomerDto?> GetByIdAsync(Guid id);
    Task<CustomerDto> CreateAsync(CreateCustomerRequest request);
    Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request);
    Task DeactivateAsync(Guid id);
}
