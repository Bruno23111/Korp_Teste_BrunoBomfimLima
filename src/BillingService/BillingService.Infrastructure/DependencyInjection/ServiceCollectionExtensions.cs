using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Persistence.Repositories;
using BillingService.Application.Invoices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillingService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddBillingInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("BillingDatabase")
            ?? throw new InvalidOperationException("Connection string 'BillingDatabase' is not configured.");

        services.AddDbContext<BillingDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IInvoiceRepository, InvoiceRepository>();

        return services;
    }
}
