namespace BillingService.Application.Invoices;

public sealed class InsufficientInvoiceStockException(
    string productCode,
    decimal availableQuantity,
    decimal requestedQuantity)
    : InvalidOperationException($"Quantidade insuficiente para o produto {productCode}. Disponível: {availableQuantity}; solicitada: {requestedQuantity}.")
{
}
