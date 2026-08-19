using BillingService.Application.Printing;
using BillingService.Domain.Entities;

namespace BillingService.Application.Invoices;

public sealed class InvoiceService(
    IInvoiceRepository invoiceRepository,
    IInventoryStockClient inventoryStockClient) : IInvoiceService
{
    public async Task<InvoiceResponse> CreateAsync(CreateInvoiceRequest request, CancellationToken cancellationToken)
    {
        if (request.Items.Count == 0)
        {
            throw new ArgumentException("An invoice must contain at least one item.", nameof(request));
        }

        var requestedQuantities = request.Items
            .GroupBy(item => item.ProductId)
            .Select(items => new { ProductId = items.Key, Quantity = items.Sum(item => item.Quantity) })
            .ToList();

        foreach (var requestedItem in requestedQuantities)
        {
            var product = await inventoryStockClient.GetProductStockAsync(requestedItem.ProductId, cancellationToken);
            if (product is null)
            {
                throw new ArgumentException($"Product '{requestedItem.ProductId}' was not found.", nameof(request));
            }

            if (requestedItem.Quantity > product.AvailableQuantity)
            {
                throw new InsufficientInvoiceStockException(product.Code, product.AvailableQuantity, requestedItem.Quantity);
            }
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

    public async Task<InvoiceResponse?> CancelAsync(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (invoice is null)
        {
            return null;
        }

        invoice.Cancel();
        await invoiceRepository.UpdateAsync(invoice, cancellationToken);
        return ToResponse(invoice);
    }

    public Task<bool> HasProductAsync(Guid productId, CancellationToken cancellationToken) =>
        invoiceRepository.ExistsByProductIdAsync(productId, cancellationToken);

    private static InvoiceResponse ToResponse(Invoice invoice) => new(
        invoice.Id, invoice.Number, invoice.Status, invoice.CreatedAt,
        invoice.Items.Select(item => new InvoiceItemResponse(item.Id, item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToList());
}
