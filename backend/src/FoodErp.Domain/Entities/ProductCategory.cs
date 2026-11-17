using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public class ProductCategory : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public Guid? ParentCategoryId { get; private set; }
    public ProductCategory? ParentCategory { get; private set; }
    public ICollection<ProductCategory> SubCategories { get; private set; } = new List<ProductCategory>();
    public ICollection<Product> Products { get; private set; } = new List<Product>();
    public bool IsActive { get; private set; } = true;

    private ProductCategory() { }

    public static ProductCategory Create(string name, Guid tenantId, string? description = null, Guid? parentCategoryId = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(name);
        return new ProductCategory
        {
            Name = name.Trim(),
            TenantId = tenantId,
            Description = description?.Trim(),
            ParentCategoryId = parentCategoryId
        };
    }

    public void Update(string name, string? description, Guid? parentCategoryId)
    {
        Name = name.Trim();
        Description = description?.Trim();
        ParentCategoryId = parentCategoryId;
        SetUpdatedAt();
    }

    public void Deactivate()
    {
        IsActive = false;
        SetUpdatedAt();
    }
}