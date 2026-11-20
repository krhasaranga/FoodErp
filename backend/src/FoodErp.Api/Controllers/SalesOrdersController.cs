using FoodErp.Application.DTOs.Distribution;
using FoodErp.Application.Interfaces.Distribution;
using FoodErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/sales-orders")]
[Authorize]
public class SalesOrdersController : ControllerBase
{
    private readonly ISalesOrderService _service;

    public SalesOrdersController(ISalesOrderService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] SalesOrderStatus? status,
        [FromQuery] Guid? customerId)
        => Ok(await _service.GetAllAsync(status, customerId));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateSalesOrderRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update delivery details while order is still Draft.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateSalesOrderRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    /// <summary>Lock the order — no further edits allowed after this.</summary>
    [HttpPost("{id:guid}/confirm")]
    public async Task<IActionResult> Confirm(Guid id)
        => Ok(await _service.ConfirmAsync(id));

    /// <summary>Issue stock and mark order as dispatched.</summary>
    [HttpPost("{id:guid}/dispatch")]
    public async Task<IActionResult> Dispatch(Guid id, [FromBody] DispatchOrderRequest request)
        => Ok(await _service.DispatchAsync(id, request));

    /// <summary>Mark the order as received by the customer.</summary>
    [HttpPost("{id:guid}/deliver")]
    public async Task<IActionResult> MarkDelivered(Guid id)
        => Ok(await _service.MarkDeliveredAsync(id));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
        => Ok(await _service.CancelAsync(id));
}
