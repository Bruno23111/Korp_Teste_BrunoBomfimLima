namespace InventoryService.Application.Stock;

public sealed class StockOperationKeyConflictException(string operationKey)
    : InvalidOperationException($"Operation key '{operationKey}' was already used with different stock items.");
