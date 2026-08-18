namespace InventoryService.Application.Products;

public sealed class ProductHasInvoicesException(Guid productId)
    : InvalidOperationException($"Product '{productId}' is linked to an invoice and cannot be changed or deleted.");
