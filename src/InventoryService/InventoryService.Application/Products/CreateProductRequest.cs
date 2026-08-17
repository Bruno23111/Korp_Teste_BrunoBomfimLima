namespace InventoryService.Application.Products;

public sealed record CreateProductRequest(string Code, string Description, decimal AvailableQuantity);
