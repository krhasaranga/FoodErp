namespace FoodErp.Application.Interfaces;

/// <summary>
/// Provides the current tenant's identity to application services and
/// infrastructure components. Resolved per HTTP request.
/// </summary>
public interface ITenantContext
{
    Guid TenantId { get; }
    bool IsResolved { get; }
}