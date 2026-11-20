using FoodErp.Application.DTOs.Admin;
using FoodErp.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

/// <summary>
/// Platform-level administration — restricted to PlatformAdmin role.
/// Regular tenant users cannot access any endpoint in this controller.
/// </summary>
[ApiController]
[Route("api/admin")]
[Authorize(Roles = "PlatformAdmin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _admin;

    public AdminController(IAdminService admin) => _admin = admin;

    // ── Platform Monitoring ────────────────────────────────────────────────────

    [HttpGet("overview")]
    public async Task<IActionResult> Overview()
        => Ok(await _admin.GetPlatformOverviewAsync());

    [HttpGet("tenants")]
    public async Task<IActionResult> AllTenants()
        => Ok(await _admin.GetAllTenantsAsync());

    [HttpGet("tenants/{tenantId:guid}/subscription")]
    public async Task<IActionResult> GetSubscription(Guid tenantId)
    {
        var result = await _admin.GetSubscriptionByTenantAsync(tenantId);
        return result is null ? NotFound() : Ok(result);
    }

    // ── Subscription Plans ─────────────────────────────────────────────────────

    [HttpGet("plans")]
    public async Task<IActionResult> GetPlans()
        => Ok(await _admin.GetPlansAsync());

    [HttpGet("plans/{id:guid}")]
    public async Task<IActionResult> GetPlan(Guid id)
    {
        var result = await _admin.GetPlanByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [HttpPost("plans")]
    public async Task<IActionResult> CreatePlan([FromBody] CreateSubscriptionPlanRequest request)
    {
        var result = await _admin.CreatePlanAsync(request);
        return CreatedAtAction(nameof(GetPlan), new { id = result.Id }, result);
    }

    [HttpPut("plans/{id:guid}")]
    public async Task<IActionResult> UpdatePlan(Guid id, [FromBody] UpdateSubscriptionPlanRequest request)
        => Ok(await _admin.UpdatePlanAsync(id, request));

    [HttpDelete("plans/{id:guid}")]
    public async Task<IActionResult> DeactivatePlan(Guid id)
    {
        await _admin.DeactivatePlanAsync(id);
        return NoContent();
    }

    // ── Tenant Subscription Management ────────────────────────────────────────

    [HttpPost("subscriptions/assign")]
    public async Task<IActionResult> Assign([FromBody] AssignSubscriptionRequest request)
        => Ok(await _admin.AssignSubscriptionAsync(request));

    [HttpPut("tenants/{tenantId:guid}/subscription/plan")]
    public async Task<IActionResult> ChangePlan(Guid tenantId, [FromBody] ChangePlanRequest request)
        => Ok(await _admin.ChangePlanAsync(tenantId, request));

    [HttpPost("tenants/{tenantId:guid}/subscription/renew")]
    public async Task<IActionResult> Renew(Guid tenantId, [FromBody] RenewSubscriptionRequest request)
        => Ok(await _admin.RenewAsync(tenantId, request));

    [HttpPost("tenants/{tenantId:guid}/subscription/suspend")]
    public async Task<IActionResult> Suspend(Guid tenantId, [FromBody] SuspendSubscriptionRequest request)
        => Ok(await _admin.SuspendAsync(tenantId, request));

    [HttpPost("tenants/{tenantId:guid}/subscription/cancel")]
    public async Task<IActionResult> Cancel(Guid tenantId, [FromBody] CancelSubscriptionRequest request)
        => Ok(await _admin.CancelAsync(tenantId, request));
}
