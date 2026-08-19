using BillingService.Domain.Entities;

namespace BillingService.Domain.Tests;

public sealed class InvoiceTests
{
    [Fact]
    public void AddItem_WhenInvoiceIsOpen_AddsProductSnapshot()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);
        var productId = Guid.NewGuid();

        invoice.AddItem(productId, "PRD-001", "Notebook", 2);

        var item = Assert.Single(invoice.Items);
        Assert.Equal(productId, item.ProductId);
        Assert.Equal("PRD-001", item.ProductCode);
        Assert.Equal("Notebook", item.ProductDescription);
        Assert.Equal(2, item.Quantity);
    }

    [Fact]
    public void Close_WhenInvoiceHasItems_ChangesStatusToClosed()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PRD-001", "Notebook", 2);

        invoice.Close();

        Assert.Equal(InvoiceStatus.Closed, invoice.Status);
    }

    [Fact]
    public void Close_WhenInvoiceHasNoItems_Throws()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);

        Assert.Throws<InvalidOperationException>(invoice.Close);
        Assert.Equal(InvoiceStatus.Open, invoice.Status);
    }

    [Fact]
    public void AddItem_WhenInvoiceIsClosed_Throws()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PRD-001", "Notebook", 2);
        invoice.Close();

        Assert.Throws<InvalidOperationException>(() =>
            invoice.AddItem(Guid.NewGuid(), "PRD-002", "Mouse", 1));
    }

    [Fact]
    public void Cancel_WhenInvoiceIsOpen_ChangesStatusToCancelled()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);

        invoice.Cancel();

        Assert.Equal(InvoiceStatus.Cancelled, invoice.Status);
    }

    [Fact]
    public void Cancel_WhenInvoiceIsClosed_Throws()
    {
        var invoice = new Invoice(1, DateTimeOffset.UtcNow);
        invoice.AddItem(Guid.NewGuid(), "PRD-001", "Notebook", 1);
        invoice.Close();

        Assert.Throws<InvalidOperationException>(invoice.Cancel);
    }
}
