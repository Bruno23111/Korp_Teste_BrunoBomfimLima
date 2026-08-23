using BillingService.Application.Printing;
using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class PrintOperationRepository(BillingDbContext context) : IPrintOperationRepository
{
    public Task<PrintOperation?> GetByInvoiceAndKeyAsync(Guid invoiceId, string idempotencyKey, CancellationToken cancellationToken) =>
        context.PrintOperations.SingleOrDefaultAsync(operation => operation.InvoiceId == invoiceId && operation.IdempotencyKey == idempotencyKey, cancellationToken);
    public void Add(PrintOperation operation) => context.PrintOperations.Add(operation);
}
