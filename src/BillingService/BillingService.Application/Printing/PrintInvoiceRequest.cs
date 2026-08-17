namespace BillingService.Application.Printing;

public sealed record PrintInvoiceRequest(Guid InvoiceId, string IdempotencyKey);
