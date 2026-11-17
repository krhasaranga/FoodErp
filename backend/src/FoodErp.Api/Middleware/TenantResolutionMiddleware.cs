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
        // Extract tenantId from the authenticated JWT token claim.
        // This is secure — the claim comes from our own signed token,
        // not from anything the client can forge.
        var tenantIdClaim = context.User.FindFirst("tenantId")?.Value;

        if (tenantIdClaim != null && Guid.TryParse(tenantIdClaim, out var tenantId))
        {
            tenantContext.SetTenant(tenantId);
        }

        await _next(context);
    }
}