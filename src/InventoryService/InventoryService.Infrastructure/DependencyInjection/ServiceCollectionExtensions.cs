using InventoryService.Infrastructure.Persistence;
using InventoryService.Infrastructure.Persistence.Repositories;
using InventoryService.Application.Products;
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
        services.AddScoped<IProductRepository, ProductRepository>();

        return services;
    }
}
