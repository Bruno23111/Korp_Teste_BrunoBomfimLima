namespace InventoryService.Application.Products;

public sealed record UpdateProductRequest(Guid Id, string Code, string Description, decimal AvailableQuantity, decimal UnitPrice);
