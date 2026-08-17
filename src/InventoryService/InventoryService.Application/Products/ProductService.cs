using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products;

public sealed class ProductService(IProductRepository productRepository) : IProductService
{
    public async Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken)
    {
        var product = new Product(request.Code, request.Description, request.AvailableQuantity);

        if (await productRepository.ExistsByCodeAsync(product.Code, cancellationToken))
        {
            throw new ProductCodeAlreadyExistsException(product.Code);
        }

        await productRepository.AddAsync(product, cancellationToken);

        return ToResponse(product);
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
}
