namespace InventoryService.Application.Products;

public sealed class ProductCodeAlreadyExistsException(string code)
    : InvalidOperationException($"Product code '{code}' is already registered.");
