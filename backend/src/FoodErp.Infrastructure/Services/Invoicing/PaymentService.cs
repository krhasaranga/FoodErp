using FoodErp.Application.DTOs.Invoicing;
using FoodErp.Application.Interfaces;
using FoodErp.Application.Interfaces.Invoicing;
using FoodErp.Domain.Entities;
using FoodErp.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FoodErp.Infrastructure.Services.Invoicing;

public class PaymentService : IPaymentService
{
    private readonly AppDbContext _db;
    private readonly ITenantContext _tenant;

    public PaymentService(AppDbContext db, ITenantContext tenant)
    {
        _db = db;
        _tenant = tenant;
    }

    public async Task<IEnumerable<PaymentDto>> GetByInvoiceAsync(Guid invoiceId)
    {
        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == invoiceId)
            ?? throw new KeyNotFoundException($"Invoice {invoiceId} not found.");

        return invoice.Payments
            .OrderByDescending(p => p.PaymentDate)
            .Select(p => new PaymentDto(
                p.Id, p.InvoiceId, invoice.InvoiceNumber,
                p.Amount, p.Method, p.PaymentDate,
                p.Reference, p.Notes, p.CreatedAt));
    }

    public async Task<PaymentDto> RecordAsync(RecordPaymentRequest request)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        // Load invoice with tracking so ApplyPayment mutation is saved
        var invoice = await _db.Invoices
            .Include(i => i.Payments)
            .FirstOrDefaultAsync(i => i.Id == request.InvoiceId)
            ?? throw new KeyNotFoundException($"Invoice {request.InvoiceId} not found.");

        // Domain validates: not cancelled, not overpaid
        invoice.ApplyPayment(request.Amount);

        var payment = Payment.Create(
            request.InvoiceId,
            request.Amount,
            request.Method,
            request.PaymentDate,
            _tenant.TenantId,
            request.Reference,
            request.Notes);

        _db.Payments.Add(payment);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();

        return new PaymentDto(
            payment.Id, payment.InvoiceId, invoice.InvoiceNumber,
            payment.Amount, payment.Method, payment.PaymentDate,
            payment.Reference, payment.Notes, payment.CreatedAt);
    }

    public async Task DeleteAsync(Guid paymentId)
    {
        await using var tx = await _db.Database.BeginTransactionAsync();

        var payment = await _db.Payments.FindAsync(paymentId)
            ?? throw new KeyNotFoundException($"Payment {paymentId} not found.");

        var invoice = await _db.Invoices.FindAsync(payment.InvoiceId)
            ?? throw new InvalidOperationException("Associated invoice not found.");

        // Reverse the payment amount from the invoice balance
        invoice.ReversePayment(payment.Amount);

        _db.Payments.Remove(payment);
        await _db.SaveChangesAsync();
        await tx.CommitAsync();
    }
}
