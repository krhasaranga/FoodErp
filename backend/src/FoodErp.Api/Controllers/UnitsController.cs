using FoodErp.Application.DTOs.Products;
using FoodErp.Application.Interfaces.Products;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UnitsController : ControllerBase
{
    private readonly IUnitService _service;

    public UnitsController(IUnitService service)
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

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUnitRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpGet("conversions")]
    public async Task<IActionResult> GetConversions()
        => Ok(await _service.GetConversionsAsync());

    [HttpPost("conversions")]
    public async Task<IActionResult> CreateConversion([FromBody] CreateUnitConversionRequest request)
    {
        var result = await _service.CreateConversionAsync(request);
        return CreatedAtAction(nameof(GetConversions), result);
    }
}