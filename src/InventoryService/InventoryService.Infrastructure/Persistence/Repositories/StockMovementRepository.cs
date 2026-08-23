using InventoryService.Application.Stock;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InventoryService.Infrastructure.Persistence.Repositories;

public sealed class StockMovementRepository(InventoryDbContext context) : IStockMovementRepository
{
    public async Task<IReadOnlyList<StockMovement>> GetByOperationKeyAsync(
        string operationKey,
        CancellationToken cancellationToken)
    {
        return await context.StockMovements
            .AsNoTracking()
            .Where(movement => movement.OperationKey == operationKey)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyDictionary<Guid, decimal>> GetConsumedQuantitiesByProductIdsAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        return await context.StockMovements
            .AsNoTracking()
            .Where(movement => productIds.Contains(movement.ProductId))
            .GroupBy(movement => movement.ProductId)
            .Select(group => new { ProductId = group.Key, Quantity = group.Sum(movement => movement.Quantity) })
            .ToDictionaryAsync(item => item.ProductId, item => item.Quantity, cancellationToken);
    }

    public void AddRange(IEnumerable<StockMovement> stockMovements) =>
        context.StockMovements.AddRange(stockMovements);
}
