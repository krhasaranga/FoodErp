using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

/// <summary>
/// One product line on an Invoice.
/// UnitPrice is snapshotted at invoice creation time.
/// </summary>
public class InvoiceLine : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public Guid? ProductId { get; private set; }  // nullable for free-text lines
    public Product? Product { get; private set; }

    /// <summary>Description shown on the invoice. Auto-filled from product name if ProductId set.</summary>
    public string Description { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }
    public decimal UnitPrice { get; private set; }

    /// <summary>Discount percentage (0–100).</summary>
    public decimal DiscountPercent { get; private set; }

    public decimal LineTotal => Math.Round(
        Quantity * UnitPrice * (1 - DiscountPercent / 100m), 4);

    private InvoiceLine() { }

    public static InvoiceLine Create(
        Guid invoiceId,
        string description,
        decimal quantity,
        decimal unitPrice,
        Guid tenantId,
        Guid? productId = null,
        decimal discountPercent = 0)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(description);
        if (quantity <= 0)
            throw new ArgumentException("Quantity must be greater than zero.", nameof(quantity));
        if (unitPrice < 0)
            throw new ArgumentException("Unit price cannot be negative.", nameof(unitPrice));
        if (discountPercent < 0 || discountPercent > 100)
            throw new ArgumentException("Discount must be between 0 and 100.", nameof(discountPercent));

        return new InvoiceLine
        {
            InvoiceId       = invoiceId,
            ProductId       = productId,
            Description     = description.Trim(),
            Quantity        = quantity,
            UnitPrice       = unitPrice,
            DiscountPercent = discountPercent,
            TenantId        = tenantId
        };
    }
}
