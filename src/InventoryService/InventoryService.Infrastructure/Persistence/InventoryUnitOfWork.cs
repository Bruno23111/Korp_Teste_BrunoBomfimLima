using System.Data;
using InventoryService.Application.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;

namespace InventoryService.Infrastructure.Persistence;

public sealed class InventoryUnitOfWork(InventoryDbContext context) : IInventoryUnitOfWork
{
    public async Task<IInventoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken)
    {
        var transaction = await context.Database.BeginTransactionAsync(IsolationLevel.ReadCommitted, cancellationToken);

        return new InventoryTransaction(transaction);
    }

    public async Task SaveChangesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new StockConcurrencyException();
        }
    }

    public void ClearChanges() => context.ChangeTracker.Clear();

    private sealed class InventoryTransaction(IDbContextTransaction transaction) : IInventoryTransaction
    {
        public Task CommitAsync(CancellationToken cancellationToken) => transaction.CommitAsync(cancellationToken);

        public ValueTask DisposeAsync() => transaction.DisposeAsync();
    }
}
