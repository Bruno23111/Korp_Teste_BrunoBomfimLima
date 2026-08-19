using System.Net.Http.Json;
using InventoryService.Application.Products;
using Microsoft.AspNetCore.Http;

namespace InventoryService.Infrastructure.Clients;

public sealed class BillingInvoiceProductUsageClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IInvoiceProductUsageClient
{
    public async Task<bool> HasInvoiceForProductAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/invoices/products/{productId}/exists");
        var authorization = httpContextAccessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization)) request.Headers.TryAddWithoutValidation("Authorization", authorization);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<bool>(cancellationToken) || false;
    }
}
