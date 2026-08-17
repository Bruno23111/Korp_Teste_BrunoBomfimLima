namespace InventoryService.Application.Products;

public interface IProductService
{
    Task<ProductResponse> CreateAsync(CreateProductRequest request, CancellationToken cancellationToken);

    Task<ProductResponse?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<ProductResponse>> GetAllAsync(CancellationToken cancellationToken);
}
