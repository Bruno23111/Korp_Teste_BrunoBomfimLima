using InventoryService.Domain.Entities;

namespace InventoryService.Domain.Tests;

public sealed class ProductTests
{
    [Fact]
    public void DecreaseStock_WhenQuantityIsAvailable_DecreasesBalanceAndCreatesMovement()
    {
        var product = new Product("PRD-001", "Notebook", 10, 100);
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
        var product = new Product("PRD-001", "Notebook", 1, 100);

        Assert.Throws<InsufficientStockException>(() =>
            product.DecreaseStock(2, "print-invoice-1", DateTimeOffset.UtcNow));

        Assert.Equal(1, product.AvailableQuantity);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void DecreaseStock_WhenQuantityIsNotPositive_Throws(decimal quantity)
    {
        var product = new Product("PRD-001", "Notebook", 1, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() =>
            product.DecreaseStock(quantity, "print-invoice-1", DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Update_WhenDataIsValid_UpdatesProductInformation()
    {
        var product = new Product("PRD-001", "Notebook", 1, 100);

        product.Update("PRD-002", "Notebook atualizado", 5, 120);

        Assert.Equal("PRD-002", product.Code);
        Assert.Equal("Notebook atualizado", product.Description);
        Assert.Equal(5, product.AvailableQuantity);
        Assert.Equal(120, product.UnitPrice);
    }

    [Fact]
    public void Update_WhenQuantityIsNegative_ThrowsAndPreservesProductInformation()
    {
        var product = new Product("PRD-001", "Notebook", 1, 100);

        Assert.Throws<ArgumentOutOfRangeException>(() => product.Update("PRD-002", "Notebook atualizado", -1, 120));

        Assert.Equal("PRD-001", product.Code);
        Assert.Equal("Notebook", product.Description);
        Assert.Equal(1, product.AvailableQuantity);
    }
}
