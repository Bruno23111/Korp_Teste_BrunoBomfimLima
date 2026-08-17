namespace BillingService.Domain.Entities;

public sealed class InvoiceItem
{
    private InvoiceItem()
    {
    }

    internal InvoiceItem(Guid productId, string productCode, string productDescription, decimal quantity)
    {
        if (productId == Guid.Empty)
        {
            throw new ArgumentException("Product identifier is required.", nameof(productId));
        }

        ProductCode = ValidateRequiredText(productCode, nameof(productCode));
        ProductDescription = ValidateRequiredText(productDescription, nameof(productDescription));

        if (quantity <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(quantity), "Quantity must be greater than zero.");
        }

        Id = Guid.NewGuid();
        ProductId = productId;
        Quantity = quantity;
    }

    public Guid Id { get; private set; }

    public Guid ProductId { get; private set; }

    public string ProductCode { get; private set; } = string.Empty;

    public string ProductDescription { get; private set; } = string.Empty;

    public decimal Quantity { get; private set; }

    private static string ValidateRequiredText(string value, string parameterName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", parameterName);
        }

        return value.Trim();
    }
}
