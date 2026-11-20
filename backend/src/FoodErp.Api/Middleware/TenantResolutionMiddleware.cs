using FoodErp.Infrastructure.Persistence;

namespace FoodErp.Api.Middleware;

public class TenantResolutionMiddleware
{
    private readonly RequestDelegate _next;

    public TenantResolutionMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context, TenantContext tenantContext)
    {
        // 1. Try to extract tenantId from the authenticated JWT token claim.
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value 
                         ?? context.User.FindFirst("TenantId")?.Value;

        if (!string.IsNullOrWhiteSpace(tenantIdClaim) && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }
        // 2. Fallback to X-Tenant-Id header for tooling, background tasks, or dev testing
        else if (context.Request.Headers.TryGetValue("X-Tenant-Id", out var headerValue)
                 && Guid.TryParse(headerValue, out var headerTenantId))
        {
            tenantContext.SetTenant(headerTenantId);
        }

        await _next(context);
    }
}