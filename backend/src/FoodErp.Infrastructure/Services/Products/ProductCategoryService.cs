using FoodErp.Application.DTOs.Products;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Products;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Products;

public class ProductCategoryService : IProductCategoryService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ProductCategoryService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<CategoryDto>> GetAllAsync()
    {
        return await _db.ProductCategories
            .Where(c => c.IsActive)
            .Include(c => c.ParentCategory)
            .OrderBy(c => c.Name)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description,
                c.ParentCategoryId,
                c.ParentCategory != null ? c.ParentCategory.Name : null,
                c.IsActive, c.CreatedAt))
            .ToListAsync();
    }

    public async Task<CategoryDto?> GetByIdAsync(Guid id)
    {
        return await _db.ProductCategories
            .Where(c => c.Id == id)
            .Include(c => c.ParentCategory)
            .Select(c => new CategoryDto(
                c.Id, c.Name, c.Description,
                c.ParentCategoryId,
                c.ParentCategory != null ? c.ParentCategory.Name : null,
                c.IsActive, c.CreatedAt))
            .FirstOrDefaultAsync();
    }

    public async Task<CategoryDto> CreateAsync(CreateCategoryRequest request)
    {
        var category = ProductCategory.Create(
            request.Name, _tenant.TenantId,
            request.Description, request.ParentCategoryId);

        _db.ProductCategories.Add(category);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(category.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created category.");
    }

    public async Task<CategoryDto> UpdateAsync(Guid id, UpdateCategoryRequest request)
    {
        var category = await _db.ProductCategories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Update(request.Name, request.Description, request.ParentCategoryId);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated category.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        var category = await _db.ProductCategories.FindAsync(id)
            ?? throw new KeyNotFoundException($"Category {id} not found.");

        category.Deactivate();
        await _db.SaveChangesAsync();
    }
}
