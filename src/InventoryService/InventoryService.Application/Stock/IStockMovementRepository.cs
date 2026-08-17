using InventoryService.Domain.Entities;

namespace InventoryService.Application.Stock;

public interface IStockMovementRepository
{
    Task<IReadOnlyList<StockMovement>> GetByOperationKeyAsync(string operationKey, CancellationToken cancellationToken);

    void AddRange(IEnumerable<StockMovement> stockMovements);
}
