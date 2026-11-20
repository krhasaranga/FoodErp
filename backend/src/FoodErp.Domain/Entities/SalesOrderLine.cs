using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// One product line on a SalesOrder.
/// UnitPrice is SNAPSHOTTED at the time the line is added — price changes on the
/// Product will not affect existing order lines.
/// </summary>
public class SalesOrderLine : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid SalesOrderId { get; private set; }
    public SalesOrder? SalesOrder { get; private set; }

    public Guid ProductId { get; private set; }
    public Product? Product { get; private set; }

    public Guid UnitId { get; private set; }
    public Unit? Unit { get; private set; }

    public decimal Quantity { get; private set; }

    /// <summary>Price per unit at the time the order was created.</summary>
    public decimal UnitPrice { get; private set; }

    /// <summary>Discount percentage (0–100).</summary>
    public decimal DiscountPercent { get; private set; }

    /// <summary>Computed: Quantity × UnitPrice × (1 − DiscountPercent/100).</summary>
    public decimal LineTotal => Math.Round(
        Quantity * UnitPrice * (1 - DiscountPercent / 100m), 4);

    private SalesOrderLine() { }

    public static SalesOrderLine Create(
        Guid salesOrderId,
        Guid productId,
        Guid unitId,
        decimal quantity,
        decimal unitPrice,
        Guid tenantId,
        decimal discountPercent = 0)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount must be between 0 and 100.", nameof(discountPercent));

        return new SalesOrderLine
        {
            SalesOrderId    = salesOrderId,
            ProductId       = productId,
            UnitId          = unitId,
            Quantity        = quantity,
            UnitPrice       = unitPrice,
            DiscountPercent = discountPercent,
            TenantId        = tenantId
        };
    }

    public void Update(decimal quantity, decimal unitPrice, decimal discountPercent)
    {
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount must be between 0 and 100.", nameof(discountPercent));

        Quantity        = quantity;
        UnitPrice       = unitPrice;
        DiscountPercent = discountPercent;
        SetUpdatedAt();
    }
}
