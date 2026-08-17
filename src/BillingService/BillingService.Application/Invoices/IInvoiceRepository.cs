using BillingService.Domain.Entities;

namespace BillingService.Application.Invoices;

public interface IInvoiceRepository
{
    Task<long> GetNextNumberAsync(CancellationToken cancellationToken);
    Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<Invoice?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);
    Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken);
    Task AddAsync(Invoice invoice, CancellationToken cancellationToken);
}
