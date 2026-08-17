using InventoryService.Application.Stock;

namespace InventoryService.Api.Contracts;

public sealed record DecreaseStockResponseDto(string OperationKey, bool AlreadyProcessed)
{
    public static DecreaseStockResponseDto From(DecreaseStockResponse response) =>
        new(response.OperationKey, response.AlreadyProcessed);
}
