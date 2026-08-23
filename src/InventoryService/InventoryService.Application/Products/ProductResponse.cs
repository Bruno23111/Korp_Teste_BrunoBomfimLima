namespace InventoryService.Application.Products;

public sealed record ProductResponse(
    Guid Id,
    string Code,
    string Description,
    decimal AvailableQuantity,
    decimal UnitPrice,
    decimal TotalAvailableValue,
    decimal TotalConsumedValue);
