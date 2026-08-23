using BillingService.Domain.Entities;

namespace BillingService.Domain.Tests;

public sealed class PrintOperationTests
{
    [Fact]
    public void Complete_WhenPending_MarksOperationAsCompleted()
    {
        var operation = new PrintOperation(Guid.NewGuid(), "print-001", DateTimeOffset.UtcNow);
        var completedAt = DateTimeOffset.UtcNow;

        operation.Complete(completedAt);

        Assert.Equal(PrintOperationStatus.Completed, operation.Status);
        Assert.Equal(completedAt, operation.CompletedAt);
        Assert.Equal(1, operation.Attempts);
    }

    [Fact]
    public void Retry_WhenFailed_ReturnsOperationToPendingAndIncrementsAttempts()
    {
        var operation = new PrintOperation(Guid.NewGuid(), "print-001", DateTimeOffset.UtcNow);
        operation.Fail("inventory-service-unavailable");

        operation.Retry();

        Assert.Equal(PrintOperationStatus.Pending, operation.Status);
        Assert.Equal(2, operation.Attempts);
        Assert.Null(operation.ErrorCode);
    }
}
