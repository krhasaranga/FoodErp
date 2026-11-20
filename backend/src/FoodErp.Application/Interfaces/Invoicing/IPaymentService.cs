using FoodErp.Application.DTOs.Invoicing;

namespace FoodErp.Application.Interfaces.Invoicing;

public interface IPaymentService
{
    Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId);
    Task<PaymentDto> RecordAsync(RecordPaymentRequest request);

    /// <summary>
    /// Deletes a payment and reverses its amount from the invoice balance.
    /// Only allowed if the payment was incorrectly entered.
    /// </summary>
    Task DeleteAsync(Guid paymentId);
}
