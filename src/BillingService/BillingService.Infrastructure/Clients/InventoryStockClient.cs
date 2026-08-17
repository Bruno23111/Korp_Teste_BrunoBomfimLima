using System.Net.Http.Json;
using BillingService.Application.Printing;

namespace BillingService.Infrastructure.Clients;

public sealed class InventoryStockClient(HttpClient httpClient) : IInventoryStockClient
{
    public async Task DecreaseStockAsync(string operationKey, IReadOnlyCollection<StockDecreaseItem> items, CancellationToken cancellationToken)
    {
        using var response = await httpClient.PostAsJsonAsync("api/stock/decreases", new { operationKey, items }, cancellationToken);
        response.EnsureSuccessStatusCode();
    }
}
