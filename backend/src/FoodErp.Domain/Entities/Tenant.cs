using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public class Tenant : BaseEntity
{
    public string Name { get; private set; } = string.Empty;
    public string Slug { get; private set; } = string.Empty; // URL-safe unique identifier e.g. "acme-foods"
    public bool IsActive { get; private set; } = true;
    public DateTime? DeactivatedAt { get; private set; }

    private Tenant() { } // Required by EF Core

    public static Tenant Create(string name, string slug)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        ArgumentException.ThrowIfNullOrWhiteSpace(slug);

        return new Tenant
        {
            Name = name.Trim(),
            Slug = slug.Trim().ToLowerInvariant()
        };
    }

    public void Deactivate()
    {
        IsActive = false;
        DeactivatedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }
}