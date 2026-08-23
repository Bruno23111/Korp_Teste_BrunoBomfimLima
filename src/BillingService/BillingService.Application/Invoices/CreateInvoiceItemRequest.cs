namespace BillingService.Application.Invoices;

public sealed record CreateInvoiceItemRequest(Guid ProductId, string ProductCode, string ProductDescription, decimal Quantity);
