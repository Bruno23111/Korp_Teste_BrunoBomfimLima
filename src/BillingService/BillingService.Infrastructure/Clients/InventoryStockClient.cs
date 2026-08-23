using System.Net.Http.Json;
using BillingService.Application.Printing;
using Microsoft.AspNetCore.Http;
using System.Net.Http.Headers;

namespace BillingService.Infrastructure.Clients;

public sealed class InventoryStockClient(HttpClient httpClient, IHttpContextAccessor httpContextAccessor) : IInventoryStockClient
{
    public async Task<InventoryProductStock?> GetProductStockAsync(Guid productId, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, $"api/products/{productId}");
        ForwardAuthorization(request, httpContextAccessor);
        using var response = await httpClient.SendAsync(request, cancellationToken);
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
        using var request = new HttpRequestMessage(HttpMethod.Post, "api/stock/decreases")
        {
            Content = JsonContent.Create(new { operationKey, items })
        };
        ForwardAuthorization(request, httpContextAccessor);
        using var response = await httpClient.SendAsync(request, cancellationToken);
        if (response.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new InventoryStockRejectedException();
        }
        response.EnsureSuccessStatusCode();
    }

    private static void ForwardAuthorization(HttpRequestMessage request, IHttpContextAccessor accessor)
    {
        var authorization = accessor.HttpContext?.Request.Headers.Authorization.ToString();
        if (!string.IsNullOrWhiteSpace(authorization))
        {
            request.Headers.TryAddWithoutValidation("Authorization", authorization);
        }
    }

    private sealed record InventoryProductDto(Guid Id, string Code, string Description, decimal AvailableQuantity);
}
