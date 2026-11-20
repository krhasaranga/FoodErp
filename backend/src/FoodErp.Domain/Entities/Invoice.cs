using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum InvoiceStatus
{
    Draft,          // being prepared, can still be edited
    Issued,         // sent to customer, locked for editing
    PartiallyPaid,  // at least one payment received but not fully settled
    Paid,           // fully settled
    Cancelled       // voided
}

/// <summary>
/// A financial document requesting payment from a customer.
/// Can be created manually or generated from a completed SalesOrder.
/// TotalAmount is recalculated from lines on save.
/// PaidAmount is updated each time a Payment is recorded.
/// </summary>
public class Invoice : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    /// <summary>Auto-generated, e.g. INV-2026-0001.</summary>
    public string InvoiceNumber { get; private set; } = string.Empty;

    public Guid CustomerId { get; private set; }
    public Customer? Customer { get; private set; }

    /// <summary>Optional link to the originating sales order.</summary>
    public Guid? SalesOrderId { get; private set; }
    public SalesOrder? SalesOrder { get; private set; }

    public InvoiceStatus Status { get; private set; } = InvoiceStatus.Draft;

    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }

    public string? Notes { get; private set; }

    /// <summary>Cached total from lines. Updated via RecalculateTotal().</summary>
    public decimal TotalAmount { get; private set; }

    /// <summary>Running sum of all recorded payments.</summary>
    public decimal PaidAmount { get; private set; }

    public decimal OutstandingAmount => TotalAmount - PaidAmount;

    public bool IsOverdue =>
        DueDate < DateTime.UtcNow
        && Status != InvoiceStatus.Paid
        && Status != InvoiceStatus.Cancelled;

    public ICollection<InvoiceLine> Lines { get; private set; } = new List<InvoiceLine>();
    public ICollection<Payment> Payments { get; private set; } = new List<Payment>();

    private Invoice() { }

    public static Invoice Create(
        string invoiceNumber,
        Guid customerId,
        DateTime invoiceDate,
        DateTime dueDate,
        Guid tenantId,
        Guid? salesOrderId = null,
        string? notes = null)
    {
        if (dueDate < invoiceDate)
            throw new ArgumentException("Due date cannot be before invoice date.");

        return new Invoice
        {
            InvoiceNumber = invoiceNumber,
            CustomerId    = customerId,
            SalesOrderId  = salesOrderId,
            InvoiceDate   = invoiceDate.ToUniversalTime(),
            DueDate       = dueDate.ToUniversalTime(),
            TenantId      = tenantId,
            Notes         = notes?.Trim()
        };
    }

    /// <summary>Recalculates TotalAmount from current lines. Call after adding/removing lines.</summary>
    public void RecalculateTotal()
    {
        TotalAmount = Lines.Sum(l => l.LineTotal);
        SetUpdatedAt();
    }

    public void Issue()
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException($"Cannot issue an invoice in '{Status}' status.");
        if (!Lines.Any())
            throw new InvalidOperationException("Cannot issue an invoice with no lines.");
        if (TotalAmount <= 0)
            throw new InvalidOperationException("Cannot issue an invoice with zero total.");

        Status = InvoiceStatus.Issued;
        SetUpdatedAt();
    }

    /// <summary>
    /// Applies a payment amount to this invoice.
    /// Updates PaidAmount and transitions status automatically.
    /// </summary>
    public void ApplyPayment(decimal amount)
    {
        if (Status == InvoiceStatus.Cancelled)
            throw new InvalidOperationException("Cannot record payment on a cancelled invoice.");
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("Invoice is already fully paid.");
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));
        if (PaidAmount + amount > TotalAmount)
            throw new InvalidOperationException(
                $"Payment of {amount} exceeds outstanding balance of {OutstandingAmount}.");

        PaidAmount += amount;
        Status = PaidAmount >= TotalAmount ? InvoiceStatus.Paid : InvoiceStatus.PartiallyPaid;
        SetUpdatedAt();
    }

    /// <summary>Reverses a payment amount (used when a payment is deleted).</summary>
    public void ReversePayment(decimal amount)
    {
        if (amount <= 0 || amount > PaidAmount)
            throw new ArgumentException("Invalid reversal amount.");

        PaidAmount -= amount;
        Status = PaidAmount <= 0
            ? InvoiceStatus.Issued
            : InvoiceStatus.PartiallyPaid;
        SetUpdatedAt();
    }

    public void Cancel()
    {
        if (Status == InvoiceStatus.Paid)
            throw new InvalidOperationException("A fully paid invoice cannot be cancelled.");

        Status = InvoiceStatus.Cancelled;
        SetUpdatedAt();
    }

    public void UpdateDraftDetails(DateTime dueDate, string? notes)
    {
        if (Status != InvoiceStatus.Draft)
            throw new InvalidOperationException("Only Draft invoices can be edited.");

        DueDate = dueDate.ToUniversalTime();
        Notes   = notes?.Trim();
        SetUpdatedAt();
    }
}
