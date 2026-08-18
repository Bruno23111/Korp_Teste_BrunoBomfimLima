using System.Net.Http.Json;
using InventoryService.Application.Products;

namespace InventoryService.Infrastructure.Clients;

public sealed class BillingInvoiceProductUsageClient(HttpClient httpClient) : IInvoiceProductUsageClient
{
    public async Task<bool> HasInvoiceForProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"api/invoices/products/{productId}/exists", cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken) || false;
    }
}
