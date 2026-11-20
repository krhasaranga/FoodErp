using FoodErp.Application.DTOs.Production;
using FoodErp.Application.Interfaces.Production;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class RecipesController : ControllerBase
{
    private readonly IRecipeService _service;

    public RecipesController(IRecipeService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-product/{productId:guid}")]
    public async Task<IActionResult> GetByProduct(Guid productId)
        => Ok(await _service.GetByProductAsync(productId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateRecipeRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRecipeRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    /// <summary>Replaces all ingredient lines of a recipe in one call.</summary>
    [HttpPut("{id:guid}/lines")]
    public async Task<IActionResult> SetLines(Guid id, [FromBody] IEnumerable<UpsertRecipeLineRequest> lines)
        => Ok(await _service.SetLinesAsync(id, lines));

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }
}
