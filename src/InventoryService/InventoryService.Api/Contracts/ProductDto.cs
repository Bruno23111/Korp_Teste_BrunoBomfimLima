using InventoryService.Application.Products;

namespace InventoryService.Api.Contracts;

public sealed record ProductDto(Guid Id, string Code, string Description, decimal AvailableQuantity)
{
    public static ProductDto From(ProductResponse product) =>
        new(product.Id, product.Code, product.Description, product.AvailableQuantity);
}
