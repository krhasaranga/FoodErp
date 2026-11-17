using FoodErp.Application.Interfaces;
using FoodErp.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using System.Linq.Expressions;

namespace FoodErp.Infrastructure.Persistence;

public static class QueryFilterExtensions
{
    public static void AddTenantQueryFilter(
        this EntityTypeBuilder builder,
        ITenantContext tenantContext)
    {
        // Builds: entity => entity.TenantId == tenantContext.TenantId
        var entityType = builder.Metadata.ClrType;
        var parameter = Expression.Parameter(entityType, "e");
        var property = Expression.Property(parameter, nameof(ITenantEntity.TenantId));
        var tenantId = Expression.Property(
            Expression.Constant(tenantContext),
            nameof(ITenantContext.TenantId));
        var equals = Expression.Equal(property, tenantId);
        var lambda = Expression.Lambda(equals, parameter);

        builder.HasQueryFilter(lambda);
    }
}