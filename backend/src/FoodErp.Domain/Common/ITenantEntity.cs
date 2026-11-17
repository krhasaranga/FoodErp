namespace FoodErp.Domain.Common;

/// <summary>
/// Marker interface for every entity that belongs to a tenant.
/// EF Core global query filters use this to enforce tenant isolation.
/// </summary>
public interface ITenantEntity
{
    Guid TenantId { get; }
}