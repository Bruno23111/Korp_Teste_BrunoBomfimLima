using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Persistence.Repositories;
using InventoryService.Infrastructure.Clients;
using InventoryService.Application.Products;
using InventoryService.Application.Stock;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InventoryService.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInventoryInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("InventoryDatabase")
            ?? throw new InvalidOperationException("Connection string 'InventoryDatabase' is not configured.");

        services.AddDbContext<InventoryDbContext>(options => options.UseNpgsql(connectionString));
        services.AddHttpContextAccessor();
        services.AddScoped<IProductRepository, ProductRepository>();
        services.AddScoped<IStockMovementRepository, StockMovementRepository>();
        services.AddScoped<IInventoryUnitOfWork, InventoryUnitOfWork>();
        services.AddHttpClient<IInvoiceProductUsageClient, BillingInvoiceProductUsageClient>(client =>
        {
            client.BaseAddress = new Uri(configuration["BillingService:BaseUrl"]
                ?? throw new InvalidOperationException("Billing service URL is not configured."));
        });

        return services;
    }
}
