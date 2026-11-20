using FoodErp.Application.DTOs.Production;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Production;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Production;

public class RecipeService : IRecipeService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public RecipeService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<RecipeListDto>> GetAllAsync()
        => await _db.Recipes
            .Include(r => r.OutputProduct)
            .Include(r => r.OutputUnit)
            .Include(r => r.Lines)
            .OrderBy(r => r.Name)
            .Select(r => new RecipeListDto(
                r.Id, r.Name,
                r.OutputProduct!.Name,
                r.OutputQuantity,
                r.OutputUnit!.Symbol,
                r.IsActive,
                r.Lines.Count))
            .ToListAsync();

    public async Task<IEnumerable<RecipeListDto>> GetByProductAsync(Guid outputProductId)
        => await _db.Recipes
            .Where(r => r.OutputProductId == outputProductId && r.IsActive)
            .Include(r => r.OutputProduct)
            .Include(r => r.OutputUnit)
            .Include(r => r.Lines)
            .Select(r => new RecipeListDto(
                r.Id, r.Name,
                r.OutputProduct!.Name,
                r.OutputQuantity,
                r.OutputUnit!.Symbol,
                r.IsActive,
                r.Lines.Count))
            .ToListAsync();

    public async Task<RecipeDto?> GetByIdAsync(Guid id)
    {
        var recipe = await _db.Recipes
            .Include(r => r.OutputProduct)
            .Include(r => r.OutputUnit)
            .Include(r => r.Lines)
                .ThenInclude(l => l.IngredientProduct)
            .Include(r => r.Lines)
                .ThenInclude(l => l.Unit)
            .FirstOrDefaultAsync(r => r.Id == id);

        return recipe is null ? null : MapToDto(recipe);
    }

    public async Task<RecipeDto> CreateAsync(CreateRecipeRequest request)
    {
        var recipe = Recipe.Create(
            request.Name,
            request.OutputProductId,
            request.OutputQuantity,
            request.OutputUnitId,
            _tenant.TenantId,
            request.Description);

        _db.Recipes.Add(recipe);
        await _db.SaveChangesAsync(); // get the recipe Id first

        // Add ingredient lines
        foreach (var line in request.Lines)
        {
            var recipeLine = RecipeLine.Create(
                recipe.Id,
                line.IngredientProductId,
                line.Quantity,
                line.UnitId,
                _tenant.TenantId,
                line.Notes);
            _db.RecipeLines.Add(recipeLine);
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(recipe.Id)
            ?? throw new InvalidOperationException("Failed to retrieve created recipe.");
    }

    public async Task<RecipeDto> UpdateAsync(Guid id, UpdateRecipeRequest request)
    {
        var recipe = await _db.Recipes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Recipe {id} not found.");

        recipe.Update(request.Name, request.Description, request.OutputQuantity, request.OutputUnitId);
        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated recipe.");
    }

    public async Task<RecipeDto> SetLinesAsync(Guid id, IEnumerable<UpsertRecipeLineRequest> lines)
    {
        var recipe = await _db.Recipes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Recipe {id} not found.");

        // Remove all existing lines and replace with new set
        var existing = await _db.RecipeLines.Where(l => l.RecipeId == id).ToListAsync();
        _db.RecipeLines.RemoveRange(existing);

        foreach (var line in lines)
        {
            var newLine = RecipeLine.Create(
                id,
                line.IngredientProductId,
                line.Quantity,
                line.UnitId,
                _tenant.TenantId,
                line.Notes);
            _db.RecipeLines.Add(newLine);
        }

        await _db.SaveChangesAsync();

        return await GetByIdAsync(id)
            ?? throw new InvalidOperationException("Failed to retrieve updated recipe.");
    }

    public async Task DeactivateAsync(Guid id)
    {
        var recipe = await _db.Recipes.FindAsync(id)
            ?? throw new KeyNotFoundException($"Recipe {id} not found.");

        recipe.Deactivate();
        await _db.SaveChangesAsync();
    }

    private static RecipeDto MapToDto(Recipe r) => new(
        r.Id, r.Name, r.Description,
        r.OutputProductId, r.OutputProduct!.Code, r.OutputProduct.Name,
        r.OutputQuantity,
        r.OutputUnitId, r.OutputUnit!.Symbol,
        r.IsActive, r.CreatedAt,
        r.Lines.Select(l => new RecipeLineDto(
            l.Id,
            l.IngredientProductId,
            l.IngredientProduct!.Code,
            l.IngredientProduct.Name,
            l.Quantity,
            l.UnitId,
            l.Unit!.Symbol,
            l.Notes)));
}
