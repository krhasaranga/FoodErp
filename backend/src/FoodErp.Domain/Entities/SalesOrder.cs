using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum SalesOrderStatus
{
    Draft,       // being prepared, can be edited freely
    Confirmed,   // locked in, ready to dispatch
    Dispatched,  // goods have left the warehouse (stock issued)
    Delivered,   // customer confirmed receipt
    Cancelled    // abandoned (stock reversed if already dispatched)
}

/// <summary>
/// A customer's order for one or more products.
/// Stock is issued from the warehouse when the order is Dispatched.
/// </summary>
public class SalesOrder : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Auto-generated, e.g. SO-2026-0001.</summary>
    public string OrderNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    /// <summary>Warehouse goods are dispatched from.</summary>
    public Guid WarehouseId { get; private set; }
    public Warehouse? Warehouse { get; private set; }

    public SalesOrderStatus Status { get; private set; } = SalesOrderStatus.Draft;

    public DateTime OrderDate { get; private set; }
    public DateTime? ExpectedDeliveryDate { get; private set; }
    public DateTime? DispatchedAt { get; private set; }
    public DateTime? DeliveredAt { get; private set; }

    public string? DeliveryAddress { get; private set; }
    public string? Notes { get; private set; }

    public ICollection<SalesOrderLine> Lines { get; private set; } = new List<SalesOrderLine>();

    private SalesOrder() { }

    public static SalesOrder Create(
        string orderNumber,
        Guid customerId,
        Guid warehouseId,
        DateTime orderDate,
        Guid tenantId,
        DateTime? expectedDeliveryDate = null,
        string? deliveryAddress = null,
        string? notes = null)
    {
        return new SalesOrder
        {
            OrderNumber          = orderNumber,
            CustomerId           = customerId,
            WarehouseId          = warehouseId,
            OrderDate            = orderDate.ToUniversalTime(),
            TenantId             = tenantId,
            ExpectedDeliveryDate = expectedDeliveryDate?.ToUniversalTime(),
            DeliveryAddress      = deliveryAddress?.Trim(),
            Notes                = notes?.Trim()
        };
    }

    public void Confirm()
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException($"Cannot confirm an order in '{Status}' status.");
        if (!Lines.Any())
            throw new InvalidOperationException("Cannot confirm an order with no lines.");

        Status = SalesOrderStatus.Confirmed;
        SetUpdatedAt();
    }

    public void Dispatch()
    {
        if (Status != SalesOrderStatus.Confirmed)
            throw new InvalidOperationException($"Cannot dispatch an order in '{Status}' status.");

        Status       = SalesOrderStatus.Dispatched;
        DispatchedAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void MarkDelivered()
    {
        if (Status != SalesOrderStatus.Dispatched)
            throw new InvalidOperationException($"Cannot mark delivered an order in '{Status}' status.");

        Status      = SalesOrderStatus.Delivered;
        DeliveredAt = DateTime.UtcNow;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == SalesOrderStatus.Delivered)
            throw new InvalidOperationException("A delivered order cannot be cancelled.");

        Status = SalesOrderStatus.Cancelled;
        SetUpdatedAt();
    }

    public void UpdateDraftDetails(
        DateTime? expectedDeliveryDate,
        string? deliveryAddress,
        string? notes)
    {
        if (Status != SalesOrderStatus.Draft)
            throw new InvalidOperationException("Only Draft orders can be edited.");

        ExpectedDeliveryDate = expectedDeliveryDate?.ToUniversalTime();
        DeliveryAddress      = deliveryAddress?.Trim();
        Notes                = notes?.Trim();
        SetUpdatedAt();
    }
}
