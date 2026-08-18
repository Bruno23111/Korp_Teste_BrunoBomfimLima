namespace InventoryService.Application.Products;

public interface IInvoiceProductUsageClient
{
    Task<bool> HasInvoiceForProductAsync(Guid productId, CancellationToken cancellationToken);
}
