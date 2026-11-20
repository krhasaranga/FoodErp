using FoodErp.Application.DTOs.Employees;

namespace FoodErp.Application.Interfaces.Employees;

public interface IEmployeeService
{
    Task<IEnumerable<EmployeeListDto>> GetAllAsync(string? department = null);
    Task<EmployeeDto?> GetByIdAsync(Guid id);
    Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request);
    Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request);
    Task DeactivateAsync(Guid id);
}
