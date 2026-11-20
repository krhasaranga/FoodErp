using FoodErp.Application.DTOs.Reports;

namespace FoodErp.Application.Interfaces.Reports;

public interface IReportService
{
    Task<DashboardDto> GetDashboardAsync();

    // Inventory
    Task<IEnumerable<StockSummaryDto>> GetStockSummaryAsync(bool lowStockOnly = false);
    Task<IEnumerable<StockMovementReportDto>> GetStockMovementsAsync(
        DateTime from, DateTime to, Guid? warehouseId = null, Guid? productId = null);

    // Sales
    Task<IEnumerable<SalesReportItemDto>> GetSalesReportAsync(DateTime from, DateTime to);
    Task<IEnumerable<SalesByCustomerDto>> GetSalesByCustomerAsync(DateTime from, DateTime to);

    // Production
    Task<IEnumerable<ProductionReportItemDto>> GetProductionReportAsync(DateTime from, DateTime to);

    // Receivables
    Task<IEnumerable<ReceivablesByCustomerDto>> GetReceivablesReportAsync();

    // Payments
    Task<IEnumerable<PaymentReportItemDto>> GetPaymentHistoryAsync(DateTime from, DateTime to);
}
