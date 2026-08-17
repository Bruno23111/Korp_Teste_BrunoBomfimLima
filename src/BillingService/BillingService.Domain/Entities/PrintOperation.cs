namespace BillingService.Domain.Entities;

public sealed class PrintOperation
{
    private PrintOperation() { }

    public PrintOperation(Guid invoiceId, string idempotencyKey, DateTimeOffset createdAt)
    {
        if (invoiceId == Guid.Empty) throw new ArgumentException("Invoice identifier is required.", nameof(invoiceId));
        if (string.IsNullOrWhiteSpace(idempotencyKey)) throw new ArgumentException("Idempotency key is required.", nameof(idempotencyKey));
        Id = Guid.NewGuid(); InvoiceId = invoiceId; IdempotencyKey = idempotencyKey.Trim(); CreatedAt = createdAt;
        Status = PrintOperationStatus.Pending; Attempts = 1;
    }

    public Guid Id { get; private set; }
    public Guid InvoiceId { get; private set; }
    public string IdempotencyKey { get; private set; } = string.Empty;
    public PrintOperationStatus Status { get; private set; }
    public int Attempts { get; private set; }
    public string? ErrorCode { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
    public DateTimeOffset? CompletedAt { get; private set; }

    public void Retry()
    {
        if (Status != PrintOperationStatus.Failed) throw new InvalidOperationException("Only failed print operations can be retried.");
        Status = PrintOperationStatus.Pending; ErrorCode = null; Attempts++;
    }

    public void Complete(DateTimeOffset completedAt)
    {
        if (Status != PrintOperationStatus.Pending) throw new InvalidOperationException("The print operation is not pending.");
        Status = PrintOperationStatus.Completed; CompletedAt = completedAt;
    }

    public void Fail(string errorCode)
    {
        if (Status != PrintOperationStatus.Pending) throw new InvalidOperationException("The print operation is not pending.");
        Status = PrintOperationStatus.Failed; ErrorCode = errorCode;
    }
}
