using FoodErp.Application.DTOs.Inventory;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Inventory;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Inventory;

public class WarehouseService : IWarehouseService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public WarehouseService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<WarehouseDto>> GetAllAsync()
        => await _db.Warehouses
            .Where(w => w.IsActive)
            .OrderBy(w => w.Name)
            .Select(w => new WarehouseDto(w.Id, w.Name, w.Location, w.IsActive, w.CreatedAt))
            .ToListAsync();

    public async Task<WarehouseDto?> GetByIdAsync(Guid id)
        => await _db.Warehouses
            .Where(w => w.Id == id)
            .Select(w => new WarehouseDto(w.Id, w.Name, w.Location, w.IsActive, w.CreatedAt))
            .FirstOrDefaultAsync();

    public async Task<WarehouseDto> CreateAsync(CreateWarehouseRequest request)
    {
        var warehouse = Warehouse.Create(request.Name, _tenant.TenantId, request.Location);
        _db.Warehouses.Add(warehouse);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(warehouse.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created warehouse.");
    }

    public async Task<WarehouseDto> UpdateAsync(Guid id, UpdateWarehouseRequest request)
    {
        var warehouse = await _db.Warehouses.FindAsync(id)
            ?? throw new KeyNotFoundException($"Warehouse {id} not found.");

        warehouse.Update(request.Name, request.Location);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated warehouse.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        var warehouse = await _db.Warehouses.FindAsync(id)
            ?? throw new KeyNotFoundException($"Warehouse {id} not found.");

        warehouse.Deactivate();
        await _db.SaveChangesAsync();
    }
}
