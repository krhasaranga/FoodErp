using FoodErp.Application.DTOs.Invoicing;
using FoodErp.Application.Interfaces.Invoicing;
using FoodErp.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class InvoicesController : ControllerBase
{
    private readonly IInvoiceService _service;

    public InvoicesController(IInvoiceService service) => _service = service;

    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] InvoiceStatus? status,
        [FromQuery] Guid? customerId,
        [FromQuery] bool overdueOnly = false)
        => Ok(await _service.GetAllAsync(status, customerId, overdueOnly));

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _service.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    /// <summary>Create a blank invoice with manually entered lines.</summary>
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateInvoiceRequest request)
    {
        var result = await _service.CreateAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>
    /// Generate an invoice from an existing SalesOrder.
    /// All lines are copied automatically.
    /// </summary>
    [HttpPost("from-sales-order")]
    public async Task<IActionResult> GenerateFromSalesOrder(
        [FromBody] GenerateInvoiceFromOrderRequest request)
    {
        var result = await _service.GenerateFromSalesOrderAsync(request);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    /// <summary>Update due date and notes while invoice is still Draft.</summary>
    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateInvoiceRequest request)
        => Ok(await _service.UpdateAsync(id, request));

    /// <summary>Issue the invoice — locks it and starts the payment clock.</summary>
    [HttpPost("{id:guid}/issue")]
    public async Task<IActionResult> Issue(Guid id)
        => Ok(await _service.IssueAsync(id));

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
        => Ok(await _service.CancelAsync(id));
}
