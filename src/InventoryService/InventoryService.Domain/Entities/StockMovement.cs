namespace InventoryService.Domain.Entities;

public sealed class StockMovement
{
    private StockMovement()
    {
    }

    internal StockMovement(Guid productId, string operationKey, decimal quantity, DateTimeOffset occurredAt)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product identifier is required.", nameof(productId));
        }

        if (string.IsNullOrWhiteSpace(operationKey))
        {
            throw new ArgumentException("Operation key is required.", nameof(operationKey));
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        OperationKey = operationKey.Trim();
        Quantity = quantity;
        OccurredAt = occurredAt;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string OperationKey { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    public DateTimeOffset OccurredAt { get; private set; }
}
