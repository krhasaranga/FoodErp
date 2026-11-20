using FoodErp.Application.DTOs.Employees;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Employees;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Employees;

public class EmployeeService : IEmployeeService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public EmployeeService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<EmployeeListDto>> GetAllAsync(string? department = null)
    {
        var query = _db.Employees.Where(e => e.IsActive).AsQueryable();

        if (!string.IsNullOrWhiteSpace(department))
            query = query.Where(e => e.Department == department);

        return await query
            .OrderBy(e => e.LastName).ThenBy(e => e.FirstName)
            .Select(e => new EmployeeListDto(
                e.Id, e.Code,
                e.FirstName + " " + e.LastName,
                e.JobTitle, e.Department,
                e.EmploymentType, e.IsActive))
            .ToListAsync();
    }

    public async Task<EmployeeDto?> GetByIdAsync(Guid id)
        => await _db.Employees
            .Where(e => e.Id == id)
            .Select(e => new EmployeeDto(
                e.Id, e.Code,
                e.FirstName, e.LastName,
                e.FirstName + " " + e.LastName,
                e.Phone, e.Email,
                e.JobTitle, e.Department,
                e.EmploymentType, e.DateOfJoining,
                e.BasicSalary, e.IsActive, e.CreatedAt))
            .FirstOrDefaultAsync();

    public async Task<EmployeeDto> CreateAsync(CreateEmployeeRequest request)
    {
        var employee = Employee.Create(
            request.Code, request.FirstName, request.LastName,
            request.EmploymentType, request.DateOfJoining, _tenant.TenantId,
            request.Phone, request.Email,
            request.JobTitle, request.Department, request.BasicSalary);

        _db.Employees.Add(employee);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(employee.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created employee.");
    }

    public async Task<EmployeeDto> UpdateAsync(Guid id, UpdateEmployeeRequest request)
    {
        var employee = await _db.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        employee.Update(
            request.FirstName, request.LastName,
            request.Phone, request.Email,
            request.JobTitle, request.Department,
            request.EmploymentType, request.BasicSalary);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated employee.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        var employee = await _db.Employees.FindAsync(id)
            ?? throw new KeyNotFoundException($"Employee {id} not found.");

        employee.Deactivate();
        await _db.SaveChangesAsync();
    }
}
