namespace InventoryService.Application.Stock;

public sealed class StockConcurrencyException()
    : InvalidOperationException("Stock was changed by another operation. Retry the request with the same operation key.");
