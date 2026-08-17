namespace InventoryService.Application.Products;

public sealed record ProductResponse(Guid Id, string Code, string Description, decimal AvailableQuantity);
