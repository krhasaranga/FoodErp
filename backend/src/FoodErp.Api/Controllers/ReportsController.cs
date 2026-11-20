using FoodErp.Application.Interfaces.Reports;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ReportsController : ControllerBase
{
    private readonly IReportService _reports;

    public ReportsController(IReportService reports) => _reports = reports;

    /// <summary>Key business metrics for the main dashboard.</summary>
    [HttpGet("dashboard")]
    public async Task<IActionResult> Dashboard()
        => Ok(await _reports.GetDashboardAsync());

    // ── Inventory ──────────────────────────────────────────────────────────────

    [HttpGet("stock/summary")]
    public async Task<IActionResult> StockSummary([FromQuery] bool lowStockOnly = false)
        => Ok(await _reports.GetStockSummaryAsync(lowStockOnly));

    [HttpGet("stock/movements")]
    public async Task<IActionResult> StockMovements(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] Guid? warehouseId,
        [FromQuery] Guid? productId)
        => Ok(await _reports.GetStockMovementsAsync(from, to, warehouseId, productId));

    // ── Sales ──────────────────────────────────────────────────────────────────

    [HttpGet("sales")]
    public async Task<IActionResult> SalesReport(
        [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _reports.GetSalesReportAsync(from, to));

    [HttpGet("sales/by-customer")]
    public async Task<IActionResult> SalesByCustomer(
        [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _reports.GetSalesByCustomerAsync(from, to));

    // ── Production ─────────────────────────────────────────────────────────────

    [HttpGet("production")]
    public async Task<IActionResult> ProductionReport(
        [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _reports.GetProductionReportAsync(from, to));

    // ── Receivables ────────────────────────────────────────────────────────────

    [HttpGet("receivables")]
    public async Task<IActionResult> Receivables()
        => Ok(await _reports.GetReceivablesReportAsync());

    // ── Payments ───────────────────────────────────────────────────────────────

    [HttpGet("payments")]
    public async Task<IActionResult> PaymentHistory(
        [FromQuery] DateTime from, [FromQuery] DateTime to)
        => Ok(await _reports.GetPaymentHistoryAsync(from, to));
}
