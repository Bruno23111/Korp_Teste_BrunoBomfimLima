namespace BillingService.Domain.Entities;

public sealed class Invoice
{
    private readonly List<InvoiceItem> _items = [];

    private Invoice()
    {
    }

    public Invoice(long number, DateTimeOffset createdAt)
    {
        if (number <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(number), "Invoice number must be greater than zero.");
        }

        Id = Guid.NewGuid();
        Number = number;
        CreatedAt = createdAt;
        Status = InvoiceStatus.Open;
    }

    public Guid Id { get; private set; }

    public long Number { get; private set; }

    public InvoiceStatus Status { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyCollection<InvoiceItem> Items => _items.AsReadOnly();

    public uint RowVersion { get; private set; }

    public void AddItem(Guid productId, string productCode, string productDescription, decimal quantity)
    {
        EnsureOpen();
        _items.Add(new InvoiceItem(productId, productCode, productDescription, quantity));
    }

    public void Close()
    {
        EnsureOpen();

        if (_items.Count == 0)
        {
            throw new InvalidOperationException("An invoice must contain at least one item before it can be closed.");
        }

        Status = InvoiceStatus.Closed;
    }

    private void EnsureOpen()
    {
        if (Status != InvoiceStatus.Open)
        {
            throw new InvalidOperationException("The invoice is not open.");
        }
    }
}
