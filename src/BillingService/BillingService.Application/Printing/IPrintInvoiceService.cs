namespace BillingService.Application.Printing;

public interface IPrintInvoiceService { Task<PrintInvoiceResponse> PrintAsync(PrintInvoiceRequest request, CancellationToken cancellationToken); }
