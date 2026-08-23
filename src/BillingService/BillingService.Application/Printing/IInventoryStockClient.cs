namespace BillingService.Application.Printing;

public interface IInventoryStockClient
{
    Task<InventoryProductStock?> GetProductStockAsync(Guid productId, CancellationToken cancellationToken);

    Task DecreaseStockAsync(string operationKey, IReadOnlyCollection<StockDecreaseItem> items, CancellationToken cancellationToken);
}

public sealed record StockDecreaseItem(Guid ProductId, decimal Quantity);

public sealed record InventoryProductStock(Guid Id, string Code, string Description, decimal AvailableQuantity);
