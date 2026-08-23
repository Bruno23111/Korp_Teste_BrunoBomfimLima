using BillingService.Application.Printing;

namespace BillingService.Infrastructure.Persistence;

public sealed class BillingUnitOfWork(BillingDbContext context) : IBillingUnitOfWork
{
    public Task SaveChangesAsync(CancellationToken cancellationToken) => context.SaveChangesAsync(cancellationToken);
}
