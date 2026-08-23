namespace BillingService.Application.Invoices;

public interface IInvoiceService
{
    Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken);
    Task<InvoiceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken);
    Task<InvoiceResponse?> CancelAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> HasProductAsync(Guid productId, CancellationToken cancellationToken);
}
