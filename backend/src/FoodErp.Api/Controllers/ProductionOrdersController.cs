using FoodErp.Application.DTOs.Production;
using FoodErp.Application.Interfaces.Production;
using FoodErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/production-orders")]
[Authorize]
public class ProductionOrdersController : ControllerBase
{
    private readonly IProductionOrderService _service;

    public ProductionOrdersController(IProductionOrderService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll([FromQuery] ProductionOrderStatus? status)
        => Ok(await _service.GetAllAsync(status));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateProductionOrderRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Move order from Draft → InProgress.</summary>
    [HttpPost("{id:guid}/start")]
    public async Task<IActionResult> Start(Guid id)
        => Ok(await _service.StartAsync(id));

    /// <summary>
    /// Complete production: issues ingredients from stock, receives finished goods.
    /// All stock movements happen in one database transaction.
    /// </summary>
    [HttpPost("{id:guid}/complete")]
    public async Task<IActionResult> Complete(Guid id, [FromBody] CompleteProductionOrderRequest request)
        => Ok(await _service.CompleteAsync(id, request));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
        => Ok(await _service.CancelAsync(id));
}
