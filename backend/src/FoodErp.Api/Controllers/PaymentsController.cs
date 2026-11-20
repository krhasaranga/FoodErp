using FoodErp.Application.DTOs.Invoicing;
using FoodErp.Application.Interfaces.Invoicing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FoodErp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _service;

    public PaymentsController(IPaymentService service) => _service = service;

    /// <summary>All payments recorded against a specific invoice.</summary>
    [HttpGet("by-invoice/{invoiceId:guid}")]
    public async Task<IActionResult> GetByInvoice(Guid invoiceId)
        => Ok(await _service.GetByInvoiceAsync(invoiceId));

    /// <summary>
    /// Record a payment against an invoice.
    /// Partial payments are supported — record multiple payments until fully paid.
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Record([FromBody] RecordPaymentRequest request)
        => Ok(await _service.RecordAsync(request));

    /// <summary>
    /// Delete an incorrectly entered payment.
    /// Automatically reverses the amount from the invoice balance.
    /// </summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await _service.DeleteAsync(id);
        return NoContent();
    }
}
