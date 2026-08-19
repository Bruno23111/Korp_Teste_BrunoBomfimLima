using System.Net.Http.Json;
using BillingService.Application.Printing;

namespace BillingService.Infrastructure.Clients;

public sealed class InventoryStockClient(HttpClient httpClient) : IInventoryStockClient
{
    public async Task<InventoryProductStock?> GetProductStockAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/products/{productId}", cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }

        response.EnsureSuccessStatusCode();
        var product = await response.Content.ReadFromJsonAsync<InventoryProductDto>(cancellationToken);

        return product is null
            ? null
            : new InventoryProductStock(product.Id, product.Code, product.Description, product.AvailableQuantity);
    }

    public async Task DecreaseStockAsync(string operationKey, IReadOnlyCollection<StockDecreaseItem> items, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/stock/decreases", new { operationKey, items }, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InventoryStockRejectedException();
        }
        response.EnsureSuccessStatusCode();
    }

    private sealed record InventoryProductDto(Guid Id, string Code, string Description, decimal AvailableQuantity);
}
