namespace InventoryService.Domain.Entities;

public sealed class InsufficientStockException(Guid productId, decimal availableQuantity, decimal requestedQuantity)
    : InvalidOperationException($"Product '{productId}' has insufficient stock. Available: {availableQuantity}; requested: {requestedQuantity}.")
{
    public Guid ProductId { get; } = productId;
}
