using BillingService.Application.Invoices;
using BillingService.Domain.Entities;

namespace BillingService.Api.Contracts;

public sealed record InvoiceDto(Guid Id, long Number, InvoiceStatus Status, DateTimeOffset CreatedAt, IReadOnlyList<InvoiceItemDto> Items)
{
    public static InvoiceDto From(InvoiceResponse invoice) => new(invoice.Id, invoice.Number, invoice.Status, invoice.CreatedAt,
        invoice.Items.Select(item => new InvoiceItemDto(item.Id, item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToList());
}

public sealed record InvoiceItemDto(Guid Id, Guid ProductId, string ProductCode, string ProductDescription, decimal Quantity);
