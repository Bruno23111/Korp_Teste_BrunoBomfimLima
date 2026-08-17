using BillingService.Application.Printing;
using BillingService.Domain.Entities;

namespace BillingService.Api.Contracts;

public sealed record PrintInvoiceResponseDto(Guid InvoiceId, PrintOperationStatus Status, int Attempts, bool AlreadyProcessed)
{
    public static PrintInvoiceResponseDto From(PrintInvoiceResponse response) =>
        new(response.InvoiceId, response.Status, response.Attempts, response.AlreadyProcessed);
}
