namespace InventoryService.Application.Stock;

public sealed record DecreaseStockRequest(string OperationKey, IReadOnlyList<DecreaseStockItem> Items);
