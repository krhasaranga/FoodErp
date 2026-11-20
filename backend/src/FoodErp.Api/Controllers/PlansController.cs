using FoodErp.Application.Interfaces.Admin;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

/// <summary>
/// Public endpoint — returns active subscription plans for display on the pricing page.
/// No authentication required.
/// </summary>
[ApiController]
[Route("api/plans")]
public class PlansController : ControllerBase
{
    private readonly IAdminService _admin;

    public PlansController(IAdminService admin) => _admin = admin;

    [HttpGet]
    public async Task<IActionResult> GetActive()
    {
        var plans = await _admin.GetPlansAsync();
        return Ok(plans.Where(p => p.IsActive));
    }
}
