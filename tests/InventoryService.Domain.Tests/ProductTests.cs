using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Tests;

public sealed class ProductTests
{
    [Fact]
    public void DecreaseStock_WhenQuantityIsAvailable_DecreasesBalanceAndCreatesMovement()
    {
        var product = new Product("PRD-001", "Notebook", 10);
        var occurredAt = new DateTimeOffset(2026, 8, 17, 12, 0, 0, TimeSpan.Zero);

        var movement = product.DecreaseStock(2, "print-invoice-1", occurredAt);

        Assert.Equal(8, product.AvailableQuantity);
        Assert.Equal(product.Id, movement.ProductId);
        Assert.Equal("print-invoice-1", movement.OperationKey);
        Assert.Equal(2, movement.Quantity);
        Assert.Equal(occurredAt, movement.OccurredAt);
    }

    [Fact]
    public void DecreaseStock_WhenQuantityExceedsBalance_ThrowsAndPreservesBalance()
    {
        var product = new Product("PRD-001", "Notebook", 1);

        Assert.Throws<InvalidOperationException>(() =>
            product.DecreaseStock(2, "print-invoice-1", DateTimeOffset.UtcNow));

        Assert.Equal(1, product.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseStock_WhenQuantityIsNotPositive_Throws(decimal quantity)
    {
        var product = new Product("PRD-001", "Notebook", 1);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.DecreaseStock(quantity, "print-invoice-1", DateTimeOffset.UtcNow));
    }
}
