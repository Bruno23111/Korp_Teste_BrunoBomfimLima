namespace InventoryService.Application.Stock;

public sealed class ProductNotFoundException(Guid productId)
    : InvalidOperationException($"Product '{productId}' was not found.")
{
    public Guid ProductId { get; } = productId;
}
