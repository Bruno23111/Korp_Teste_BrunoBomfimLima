namespace BillingService.Application.Invoices;

public sealed record InvoiceItemResponse(Guid Id, Guid ProductId, string ProductCode, string ProductDescription, decimal Quantity);
