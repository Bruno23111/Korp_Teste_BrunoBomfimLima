namespace BillingService.Application.Printing;

public interface IBillingUnitOfWork { Task SaveChangesAsync(CancellationToken cancellationToken); }
