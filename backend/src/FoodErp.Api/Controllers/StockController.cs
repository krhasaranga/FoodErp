using FoodErp.Application.DTOs.Inventory;
using FoodErp.Application.Interfaces.Inventory;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class StockController : ControllerBase
{
    private readonly IStockService _service;

    public StockController(IStockService service) => _service = service;

    // ── Queries ───────────────────────────────────────────────────────────────

    /// <summary>All current stock balances in a warehouse.</summary>
    [HttpGet("balances/warehouse/{warehouseId:guid}")]
    public async Task<IActionResult> BalancesByWarehouse(Guid warehouseId)
        => Ok(await _service.GetBalancesByWarehouseAsync(warehouseId));

    /// <summary>Balance of a product across all warehouses (total on-hand).</summary>
    [HttpGet("balances/product/{productId:guid}")]
    public async Task<IActionResult> BalancesByProduct(Guid productId)
        => Ok(await _service.GetBalancesByProductAsync(productId));

    /// <summary>Movement history. Filter by warehouseId and/or productId via query string.</summary>
    [HttpGet("movements")]
    public async Task<IActionResult> Movements(
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
        => Ok(await _service.GetMovementsAsync(warehouseId, productId, page, pageSize));

    // ── Mutations ─────────────────────────────────────────────────────────────

    /// <summary>Set the opening (initial) stock for a product in a warehouse.</summary>
    [HttpPost("opening")]
    public async Task<IActionResult> SetOpening([FromBody] OpeningStockRequest request)
        => Ok(await _service.SetOpeningStockAsync(request));

    /// <summary>Receive stock into a warehouse (purchase receipt, production output, return).</summary>
    [HttpPost("receive")]
    public async Task<IActionResult> Receive([FromBody] ReceiveStockRequest request)
        => Ok(await _service.ReceiveAsync(request));

    /// <summary>Issue stock from a warehouse (consumed in production, sold, etc.).</summary>
    [HttpPost("issue")]
    public async Task<IActionResult> Issue([FromBody] IssueStockRequest request)
        => Ok(await _service.IssueAsync(request));

    /// <summary>Adjust stock to an exact quantity (physical count correction).</summary>
    [HttpPost("adjust")]
    public async Task<IActionResult> Adjust([FromBody] AdjustStockRequest request)
        => Ok(await _service.AdjustAsync(request));

    /// <summary>Transfer stock between two warehouses atomically.</summary>
    [HttpPost("transfer")]
    public async Task<IActionResult> Transfer([FromBody] TransferStockRequest request)
    {
        var (outMovement, inMovement) = await _service.TransferAsync(request);
        return Ok(new { outMovement, inMovement });
    }
}
