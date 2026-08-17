using BillingService.Infrastructure.Persistence;
using BillingService.Infrastructure.Persistence.Repositories;
using BillingService.Infrastructure.Clients;
using BillingService.Application.Invoices;
using BillingService.Application.Printing;
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
        services.AddScoped<IPrintOperationRepository, PrintOperationRepository>();
        services.AddScoped<IBillingUnitOfWork, BillingUnitOfWork>();
        services.AddHttpClient<IInventoryStockClient, InventoryStockClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["InventoryService:BaseUrl"]
                ?? throw new InvalidOperationException("Inventory service URL is not configured."));
        });

        return services;
    }
}
