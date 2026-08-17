using BillingService.Domain.Entities;

namespace BillingService.Application.Invoices;

public sealed record InvoiceResponse(Guid Id, long Number, InvoiceStatus Status, DateTimeOffset CreatedAt, IReadOnlyList<InvoiceItemResponse> Items);
