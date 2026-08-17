namespace BillingService.Application.Printing;

public interface IInventoryStockClient
{
    Task DecreaseStockAsync(string operationKey, IReadOnlyCollection<StockDecreaseItem> items, CancellationToken cancellationToken);
}

public sealed record StockDecreaseItem(Guid ProductId, decimal Quantity);
