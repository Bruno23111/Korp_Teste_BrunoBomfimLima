using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace BillingService.Infrastructure.Persistence;

public sealed class BillingDbContextFactory : IDesignTimeDbContextFactory<BillingDbContext>
{
    public BillingDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__BillingDatabase")
            ?? "Host=localhost;Port=5434;Database=billing_db";
        var options = new DbContextOptionsBuilder<BillingDbContext>()
            .UseNpgsql(connectionString)
            .Options;
        return new BillingDbContext(options);
    }
}
