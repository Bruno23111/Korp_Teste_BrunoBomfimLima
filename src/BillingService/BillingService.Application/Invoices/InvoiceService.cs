using BillingService.Domain.Entities;

namespace BillingService.Application.Invoices;

public sealed class InvoiceService(IInvoiceRepository invoiceRepository) : IInvoiceService
{
    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new ArgumentException("An invoice must contain at least one item.", nameof(request));
        }

        var invoice = new Invoice(await invoiceRepository.GetNextNumberAsync(cancellationToken), DateTimeOffset.UtcNow);

        foreach (var item in request.Items)
        {
            invoice.AddItem(item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity);
        }

        await invoiceRepository.AddAsync(invoice, cancellationToken);
        return ToResponse(invoice);
    }

    public async Task<InvoiceResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdAsync(id, cancellationToken);
        return invoice is null ? null : ToResponse(invoice);
    }

    public async Task<IReadOnlyList<InvoiceResponse>> GetAllAsync(CancellationToken cancellationToken) =>
        (await invoiceRepository.GetAllAsync(cancellationToken)).Select(ToResponse).ToList();

    public Task<bool> HasProductAsync(Guid productId, CancellationToken cancellationToken) =>
        invoiceRepository.ExistsByProductIdAsync(productId, cancellationToken);

    private static InvoiceResponse ToResponse(Invoice invoice) => new(
        invoice.Id, invoice.Number, invoice.Status, invoice.CreatedAt,
        invoice.Items.Select(item => new InvoiceItemResponse(item.Id, item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToList());
}
