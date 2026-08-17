namespace InventoryService.Application.Stock;

public sealed record DecreaseStockItem(Guid ProductId, decimal Quantity);
