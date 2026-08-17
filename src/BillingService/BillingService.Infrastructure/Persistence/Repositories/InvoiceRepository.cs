using BillingService.Application.Invoices;
using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class InvoiceRepository(BillingDbContext context) : IInvoiceRepository
{
    public async Task<long> GetNextNumberAsync(CancellationToken cancellationToken) =>
        ((await context.Invoices.AsNoTracking().Select(invoice => (long?)invoice.Number).MaxAsync(cancellationToken)) ?? 0) + 1;

    public Task<Invoice?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Invoices.AsNoTracking().Include(invoice => invoice.Items).SingleOrDefaultAsync(invoice => invoice.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Invoice>> GetAllAsync(CancellationToken cancellationToken) =>
        await context.Invoices.AsNoTracking().Include(invoice => invoice.Items).OrderByDescending(invoice => invoice.Number).ToListAsync(cancellationToken);

    public async Task AddAsync(Invoice invoice, CancellationToken cancellationToken)
    {
        context.Invoices.Add(invoice);
        try { await context.SaveChangesAsync(cancellationToken); }
        catch (DbUpdateException exception) when (exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        { throw new InvoiceNumberAlreadyExistsException(invoice.Number); }
    }
}
