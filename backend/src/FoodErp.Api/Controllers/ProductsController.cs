using FoodErp.Application.DTOs.Products;
using FoodErp.Application.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _service;

    public ProductsController(IProductService service)
    {
        _service = service;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
        => Ok(await _service.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpGet("by-category/{categoryId:guid}")]
    public async Task<IActionResult> GetByCategory(Guid categoryId)
        => Ok(await _service.GetByCategoryAsync(categoryId));

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateProductRequest request)
    {
        var result = await _service.UpdateAsync(id, request);
        return Ok(result);
    }

    [HttpPatch("{id:guid}/prices")]
    public async Task<IActionResult> UpdatePrices(Guid id, [FromBody] UpdateProductPricesRequest request)
    {
        await _service.UpdatePricesAsync(id, request);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Deactivate(Guid id)
    {
        await _service.DeactivateAsync(id);
        return NoContent();
    }
}