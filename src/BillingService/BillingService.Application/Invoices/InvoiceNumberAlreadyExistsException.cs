namespace BillingService.Application.Invoices;

public sealed class InvoiceNumberAlreadyExistsException(long number)
    : InvalidOperationException($"Invoice number '{number}' is already registered.");
