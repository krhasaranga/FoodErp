using FoodErp.Application.DTOs.Products;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Products;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Products;

public class UnitService : IUnitService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public UnitService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<UnitDto>> GetAllAsync()
    {
        return await _db.Units
            .Where(u => u.IsActive)
            .OrderBy(u => u.Name)
            .Select(u => new UnitDto(u.Id, u.Name, u.Symbol, u.IsBaseUnit, u.IsActive))
            .ToListAsync();
    }

    public async Task<UnitDto?> GetByIdAsync(Guid id)
    {
        return await _db.Units
            .Where(u => u.Id == id)
            .Select(u => new UnitDto(u.Id, u.Name, u.Symbol, u.IsBaseUnit, u.IsActive))
            .FirstOrDefaultAsync();
    }

    public async Task<UnitDto> CreateAsync(CreateUnitRequest request)
    {
        var unit = Unit.Create(request.Name, request.Symbol, _tenant.TenantId, request.IsBaseUnit);
        _db.Units.Add(unit);
        await _db.SaveChangesAsync();

        return new UnitDto(unit.Id, unit.Name, unit.Symbol, unit.IsBaseUnit, unit.IsActive);
    }

    public async Task<IEnumerable<UnitConversionDto>> GetConversionsAsync()
    {
        return await _db.UnitConversions
            .Include(c => c.FromUnit)
            .Include(c => c.ToUnit)
            .Select(c => new UnitConversionDto(
                c.Id,
                c.FromUnitId, c.FromUnit!.Symbol,
                c.ToUnitId, c.ToUnit!.Symbol,
                c.Factor))
            .ToListAsync();
    }

    public async Task<UnitConversionDto> CreateConversionAsync(CreateUnitConversionRequest request)
    {
        var conversion = UnitConversion.Create(
            request.FromUnitId, request.ToUnitId,
            request.Factor, _tenant.TenantId);

        _db.UnitConversions.Add(conversion);
        await _db.SaveChangesAsync();

        return await _db.UnitConversions
            .Where(c => c.Id == conversion.Id)
            .Include(c => c.FromUnit)
            .Include(c => c.ToUnit)
            .Select(c => new UnitConversionDto(
                c.Id,
                c.FromUnitId, c.FromUnit!.Symbol,
                c.ToUnitId, c.ToUnit!.Symbol,
                c.Factor))
            .FirstAsync();
    }
}
