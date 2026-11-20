using FoodErp.Application.DTOs.Production;

namespace FoodErp.Application.Interfaces.Production;

public interface IRecipeService
{
    Task<IEnumerable<RecipeListDto>> GetAllAsync();
    Task<RecipeDto?> GetByIdAsync(Guid id);
    Task<IEnumerable<RecipeListDto>> GetByProductAsync(Guid outputProductId);
    Task<RecipeDto> CreateAsync(CreateRecipeRequest request);
    Task<RecipeDto> UpdateAsync(Guid id, UpdateRecipeRequest request);
    Task<RecipeDto> SetLinesAsync(Guid id, IEnumerable<UpsertRecipeLineRequest> lines);
    Task DeactivateAsync(Guid id);
}
