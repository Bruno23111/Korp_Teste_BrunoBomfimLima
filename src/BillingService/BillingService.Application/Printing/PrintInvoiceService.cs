using BillingService.Application.Invoices;
using BillingService.Domain.Entities;

namespace BillingService.Application.Printing;

public sealed class PrintInvoiceService(
    IInvoiceRepository invoiceRepository,
    IPrintOperationRepository printOperationRepository,
    IInventoryStockClient inventoryStockClient,
    IBillingUnitOfWork unitOfWork) : IPrintInvoiceService
{
    public async Task<PrintInvoiceResponse> PrintAsync(PrintInvoiceRequest request, CancellationToken cancellationToken)
    {
        var invoice = await invoiceRepository.GetByIdForUpdateAsync(request.InvoiceId, cancellationToken)
            ?? throw new InvoiceNotFoundException(request.InvoiceId);
        var operation = await printOperationRepository.GetByInvoiceAndKeyAsync(invoice.Id, request.IdempotencyKey, cancellationToken);

        if (operation?.Status == PrintOperationStatus.Completed)
            return new PrintInvoiceResponse(invoice.Id, operation.Status, operation.Attempts, true);
        if (invoice.Status != InvoiceStatus.Open) throw new InvalidOperationException("Only open invoices can be printed.");

        if (operation is null) { operation = new PrintOperation(invoice.Id, request.IdempotencyKey, DateTimeOffset.UtcNow); printOperationRepository.Add(operation); }
        else operation.Retry();

        try
        {
            await inventoryStockClient.DecreaseStockAsync($"invoice:{invoice.Id}:print:{operation.IdempotencyKey}", invoice.Items.Select(item => new StockDecreaseItem(item.ProductId, item.Quantity)).ToList(), cancellationToken);
        }
        catch (InventoryStockRejectedException)
        {
            operation.Fail("inventory-stock-rejected");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new PrintInvoiceFailedException(operation.ErrorCode!);
        }
        catch (Exception exception) when (exception is HttpRequestException or TaskCanceledException)
        {
            operation.Fail("inventory-service-unavailable");
            await unitOfWork.SaveChangesAsync(cancellationToken);
            throw new PrintInvoiceFailedException(operation.ErrorCode!);
        }

        invoice.Close();
        operation.Complete(DateTimeOffset.UtcNow);
        await unitOfWork.SaveChangesAsync(cancellationToken);
        return new PrintInvoiceResponse(invoice.Id, operation.Status, operation.Attempts, false);
    }
}
