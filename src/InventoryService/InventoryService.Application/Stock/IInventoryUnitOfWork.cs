namespace InventoryService.Application.Stock;

public interface IInventoryUnitOfWork
{
    Task<IInventoryTransaction> BeginTransactionAsync(CancellationToken cancellationToken);

    Task SaveChangesAsync(CancellationToken cancellationToken);

    void ClearChanges();
}
