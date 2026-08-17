namespace InventoryService.Domain.Entities;

public sealed class Product
{
    private Product()
    {
    }

    public Product(string code, string description, decimal availableQuantity)
    {
        Id = Guid.NewGuid();
        Code = ValidateRequiredText(code, nameof(code));
        Description = ValidateRequiredText(description, nameof(description));

        if (availableQuantity < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(availableQuantity), "Available quantity cannot be negative.");
        }

        AvailableQuantity = availableQuantity;
    }

    public Guid Id { get; private set; }

    public string Code { get; private set; } = string.Empty;

    public string Description { get; private set; } = string.Empty;

    public decimal AvailableQuantity { get; private set; }

    public uint RowVersion { get; private set; }

    public StockMovement DecreaseStock(decimal quantity, string operationKey, DateTimeOffset occurredAt)
    {
        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        if (quantity > AvailableQuantity)
        {
            throw new InvalidOperationException("Insufficient stock for this operation.");
        }

        AvailableQuantity -= quantity;

        return new StockMovement(Id, operationKey, quantity, occurredAt);
    }

    private static string ValidateRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
