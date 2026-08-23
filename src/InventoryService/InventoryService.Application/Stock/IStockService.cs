namespace InventoryService.Application.Stock;

public interface IStockService
{
    Task<DecreaseStockResponse> DecreaseAsync(DecreaseStockRequest request, CancellationToken cancellationToken);
}
