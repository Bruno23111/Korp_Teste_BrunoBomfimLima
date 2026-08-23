using BillingService.Domain.Entities;

namespace BillingService.Application.Printing;

public sealed record PrintInvoiceResponse(Guid InvoiceId, PrintOperationStatus Status, int Attempts, bool AlreadyProcessed);
