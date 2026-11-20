using FoodErp.Application.DTOs.Distribution;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Distribution;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Distribution;

public class CustomerService : ICustomerService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public CustomerService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<CustomerListDto>> GetAllAsync()
        => await _db.Customers
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new CustomerListDto(
                c.Id, c.Code, c.Name, c.Type, c.Phone, c.IsActive))
            .ToListAsync();

    public async Task<CustomerDto?> GetByIdAsync(Guid id)
        => await _db.Customers
            .Where(c => c.Id == id)
            .Select(c => new CustomerDto(
                c.Id, c.Code, c.Name, c.Type,
                c.ContactPerson, c.Phone, c.Email, c.Address,
                c.CreditLimit, c.IsActive, c.CreatedAt))
            .FirstOrDefaultAsync();

    public async Task<CustomerDto> CreateAsync(CreateCustomerRequest request)
    {
        var customer = Customer.Create(
            request.Code, request.Name, request.Type, _tenant.TenantId,
            request.ContactPerson, request.Phone, request.Email,
            request.Address, request.CreditLimit);

        _db.Customers.Add(customer);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(customer.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created customer.");
    }

    public async Task<CustomerDto> UpdateAsync(Guid id, UpdateCustomerRequest request)
    {
        var customer = await _db.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        customer.Update(request.Name, request.Type, request.ContactPerson,
            request.Phone, request.Email, request.Address, request.CreditLimit);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated customer.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        var customer = await _db.Customers.FindAsync(id)
            ?? throw new KeyNotFoundException($"Customer {id} not found.");

        customer.Deactivate();
        await _db.SaveChangesAsync();
    }
}
