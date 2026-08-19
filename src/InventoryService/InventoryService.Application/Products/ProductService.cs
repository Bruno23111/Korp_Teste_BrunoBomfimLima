using InventoryService.Application.Stock;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products;

public sealed class ProductService(
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    IInvoiceProductUsageClient invoiceProductUsageClient) : IProductService
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Code, request.Description, request.AvailableQuantity, request.UnitPrice);

        if (await productRepository.ExistsByCodeAsync(product.Code, null, cancellationToken))
        {
            throw new ProductCodeAlreadyExistsException(product.Code);
        }

        await productRepository.AddAsync(product, cancellationToken);

        return ToResponse(product);
    }

    public async Task<ProductResponse?> UpdateAsync(UpdateProductRequest request, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(request.Id, cancellationToken);
        if (product is null)
        {
            return null;
        }

        await EnsureProductIsNotUsedByInvoiceAsync(product.Id, cancellationToken);
        product.Update(request.Code, request.Description, request.AvailableQuantity, request.UnitPrice);

        if (await productRepository.ExistsByCodeAsync(product.Code, product.Id, cancellationToken))
        {
            throw new ProductCodeAlreadyExistsException(product.Code);
        }

        await productRepository.UpdateAsync(product, cancellationToken);
        return ToResponse(product);
    }

    public async Task<bool> DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdForUpdateAsync(id, cancellationToken);
        if (product is null)
        {
            return false;
        }

        await EnsureProductIsNotUsedByInvoiceAsync(product.Id, cancellationToken);
        await productRepository.DeleteAsync(product, cancellationToken);
        return true;
    }

    public async Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        var product = await productRepository.GetByIdAsync(id, cancellationToken);

        if (product is null)
        {
            return null;
        }

        var consumedQuantities = await stockMovementRepository.GetConsumedQuantitiesByProductIdsAsync([product.Id], cancellationToken);
        return ToResponse(product, consumedQuantities.GetValueOrDefault(product.Id));
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        var consumedQuantities = await stockMovementRepository.GetConsumedQuantitiesByProductIdsAsync(
            products.Select(product => product.Id).ToList(),
            cancellationToken);

        return products.Select(product => ToResponse(product, consumedQuantities.GetValueOrDefault(product.Id))).ToList();
    }

    private static ProductResponse ToResponse(Product product, decimal consumedQuantity = 0) =>
        new(
            product.Id,
            product.Code,
            product.Description,
            product.AvailableQuantity,
            product.UnitPrice,
            product.AvailableQuantity * product.UnitPrice,
            consumedQuantity * product.UnitPrice);

    private async Task EnsureProductIsNotUsedByInvoiceAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (await invoiceProductUsageClient.HasInvoiceForProductAsync(productId, cancellationToken))
        {
            throw new ProductHasInvoicesException(productId);
        }
    }
}
