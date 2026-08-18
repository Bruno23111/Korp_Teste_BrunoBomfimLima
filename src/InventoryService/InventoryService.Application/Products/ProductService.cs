using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products;

public sealed class ProductService(
    IProductRepository productRepository,
    IInvoiceProductUsageClient invoiceProductUsageClient) : IProductService
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Code, request.Description, request.AvailableQuantity);

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
        product.Update(request.Code, request.Description, request.AvailableQuantity);

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

        return product is null ? null : ToResponse(product);
    }

    public async Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken)
    {
        var products = await productRepository.GetAllAsync(cancellationToken);

        return products.Select(ToResponse).ToList();
    }

    private static ProductResponse ToResponse(Product product) =>
        new(product.Id, product.Code, product.Description, product.AvailableQuantity);

    private async Task EnsureProductIsNotUsedByInvoiceAsync(Guid productId, CancellationToken cancellationToken)
    {
        if (await invoiceProductUsageClient.HasInvoiceForProductAsync(productId, cancellationToken))
        {
            throw new ProductHasInvoicesException(productId);
        }
    }
}
