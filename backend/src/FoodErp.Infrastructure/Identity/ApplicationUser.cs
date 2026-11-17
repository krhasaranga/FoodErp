using FoodErp.Domain.Common;
using Microsoft.AspNetCore.Identity;

namespace FoodErp.Infrastructure.Identity;

public class ApplicationUser : IdentityUser<Guid>, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public bool IsActive { get; private set; } = true;
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; private set; }

    private ApplicationUser() { } // Required by EF Core

    public static ApplicationUser Create(
        string email, string firstName, string lastName, Guid tenantId)
    {
        return new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = email.ToLowerInvariant(),
            Email = email.ToLowerInvariant(),
            FirstName = firstName.Trim(),
            LastName = lastName.Trim(),
            TenantId = tenantId
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }
}