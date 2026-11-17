using FoodErp.Application.DTOs.Products;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Products;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Products;

public class ProductService : IProductService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public ProductService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    private static ProductDto MapToDto(Product p) => new(
        p.Id, p.Code, p.Name, p.Description,
        p.Type,
        p.CategoryId, p.Category!.Name,
        p.UnitId, p.Unit!.Symbol,
        p.CostPrice, p.SellingPrice,
        p.ReorderLevel, p.TrackExpiry,
        p.IsActive, p.CreatedAt);

    private IQueryable<Product> BaseQuery() =>
        _db.Products
            .Include(p => p.Category)
            .Include(p => p.Unit);

    public async Task<IEnumerable<ProductDto>> GetAllAsync()
    {
        // Materialize first with ToListAsync(), then map in memory.
        // EF Core cannot translate static C# methods to SQL directly.
        var products = await BaseQuery()
            .Where(p => p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        var product = await BaseQuery()
            .FirstOrDefaultAsync(p => p.Id == id);

        return product is null ? null : MapToDto(product);
    }

    public async Task<IEnumerable<ProductDto>> GetByCategoryAsync(Guid categoryId)
    {
        var products = await BaseQuery()
            .Where(p => p.CategoryId == categoryId && p.IsActive)
            .OrderBy(p => p.Name)
            .ToListAsync();

        return products.Select(MapToDto);
    }

    public async Task<ProductDto> CreateAsync(CreateProductRequest request)
    {
        var product = Product.Create(
            request.Code, request.Name, request.Type,
            request.CategoryId, request.UnitId, _tenant.TenantId,
            request.CostPrice, request.SellingPrice,
            request.ReorderLevel, request.TrackExpiry, request.Description);

        _db.Products.Add(product);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(product.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created product.");
    }

    public async Task<ProductDto> UpdateAsync(Guid id, UpdateProductRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Update(request.Code, request.Name, request.Description,
            request.CategoryId, request.UnitId, request.ReorderLevel, request.TrackExpiry);

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated product.");
    }

    public async Task UpdatePricesAsync(Guid id, UpdateProductPricesRequest request)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.UpdatePrices(request.CostPrice, request.SellingPrice);
        await _db.SaveChangesAsync();
    }

    public async Task DeactivateAsync(Guid id)
    {
        var product = await _db.Products.FindAsync(id)
            ?? throw new KeyNotFoundException($"Product {id} not found.");

        product.Deactivate();
        await _db.SaveChangesAsync();
    }
}
