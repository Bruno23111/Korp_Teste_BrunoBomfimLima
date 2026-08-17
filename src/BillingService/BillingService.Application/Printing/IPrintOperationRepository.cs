using BillingService.Domain.Entities;

namespace BillingService.Application.Printing;

public interface IPrintOperationRepository
{
    Task<PrintOperation?> GetByInvoiceAndKeyAsync(Guid invoiceId, string idempotencyKey, CancellationToken cancellationToken);
    void Add(PrintOperation operation);
}
