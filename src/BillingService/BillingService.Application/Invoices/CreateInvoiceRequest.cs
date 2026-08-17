namespace BillingService.Application.Invoices;

public sealed record CreateInvoiceRequest(IReadOnlyCollection<CreateInvoiceItemRequest> Items);
