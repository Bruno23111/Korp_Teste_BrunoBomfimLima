namespace InventoryService.Application.Stock;

public sealed record DecreaseStockResponse(string OperationKey, bool AlreadyProcessed);
