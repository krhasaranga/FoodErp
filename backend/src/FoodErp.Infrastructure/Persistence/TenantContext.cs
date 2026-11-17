using FoodErp.Application.Interfaces;
using Microsoft.AspNetCore.Http;

namespace FoodErp.Infrastructure.Persistence;

public class TenantContext : ITenantContext
{
    public Guid TenantId { get; private set; }
    public bool IsResolved { get; private set; }

    public void SetTenant(Guid tenantId)
    {
        TenantId = tenantId;
        IsResolved = true;
    }
}