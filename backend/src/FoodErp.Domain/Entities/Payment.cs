using FoodErp.Domain.Common;

namespace FoodErp.Domain.Entities;

public enum PaymentMethod
{
    Cash,
    BankTransfer,
    Cheque,
    MobileMoney,
    Other
}

/// <summary>
/// A single payment recorded against an Invoice.
/// Payments are immutable — to correct a mistake, delete and re-record.
/// Deleting a payment reverses its amount from Invoice.PaidAmount.
/// </summary>
public class Payment : BaseEntity, ITenantEntity
{
    public Guid TenantId { get; private set; }

    public Guid InvoiceId { get; private set; }
    public Invoice? Invoice { get; private set; }

    public decimal Amount { get; private set; }
    public PaymentMethod Method { get; private set; }
    public DateTime PaymentDate { get; private set; }

    /// <summary>Cheque number, bank transaction ID, mobile money ref, etc.</summary>
    public string? Reference { get; private set; }

    public string? Notes { get; private set; }

    private Payment() { }

    public static Payment Create(
        Guid invoiceId,
        decimal amount,
        PaymentMethod method,
        DateTime paymentDate,
        Guid tenantId,
        string? reference = null,
        string? notes = null)
    {
        if (amount <= 0)
            throw new ArgumentException("Payment amount must be greater than zero.", nameof(amount));

        return new Payment
        {
            InvoiceId   = invoiceId,
            Amount      = amount,
            Method      = method,
            PaymentDate = paymentDate.ToUniversalTime(),
            TenantId    = tenantId,
            Reference   = reference?.Trim(),
            Notes       = notes?.Trim()
        };
    }
}
