using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Identity;
using FoodErp.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly TokenService _tokenService;
    private readonly AppDbContext _db;

    public AuthController(
        UserManager<ApplicationUser> userManager,
        SignInManager<ApplicationUser> signInManager,
        TokenService tokenService,
        AppDbContext db)
    {
        _userManager = userManager;
        _signInManager = signInManager;
        _tokenService = tokenService;
        _db = db;
    }

    /// <summary>
    /// Register a new tenant and its first admin user.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Create the tenant
        var tenant = Tenant.Create(request.TenantName, request.TenantSlug);
        _db.Tenants.Add(tenant);
        await _db.SaveChangesAsync();

        // 2. Create the admin user for this tenant
        var user = ApplicationUser.Create(
            request.Email, request.FirstName, request.LastName, tenant.Id);

        var result = await _userManager.CreateAsync(user, request.Password);

        if (!result.Succeeded)
        {
            // Roll back tenant creation
            _db.Tenants.Remove(tenant);
            await _db.SaveChangesAsync();
            return BadRequest(result.Errors);
        }

        await _userManager.AddToRoleAsync(user, "TenantAdmin");

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new
        {
            token,
            tenantId = tenant.Id,
            userId = user.Id,
            email = user.Email,
            roles,
            message = "Tenant registered successfully."
        });
    }

    /// <summary>
    /// Login and receive a JWT token.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // Bypass the global query filter — login happens before tenant is known
        var user = await _userManager.Users
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(u => u.Email == request.Email.ToLowerInvariant());

        if (user == null || !user.IsActive)
            return Unauthorized(new { message = "Invalid credentials." });

        var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, lockoutOnFailure: true);

        if (!result.Succeeded)
            return Unauthorized(new { message = "Invalid credentials." });

        var roles = await _userManager.GetRolesAsync(user);
        var token = _tokenService.GenerateToken(user, roles);

        return Ok(new
        {
            token,
            tenantId = user.TenantId,
            userId = user.Id,
            email = user.Email,
            roles
        });
    }
}

public record RegisterRequest(
    string TenantName,
    string TenantSlug,
    string FirstName,
    string LastName,
    string Email,
    string Password);

public record LoginRequest(string Email, string Password);