namespace BillingService.Application.Printing;

public sealed class InvoiceNotFoundException(Guid invoiceId) : InvalidOperationException($"Invoice '{invoiceId}' was not found.");
public sealed class PrintInvoiceFailedException(string errorCode) : InvalidOperationException(errorCode) { public string ErrorCode { get; } = errorCode; }
