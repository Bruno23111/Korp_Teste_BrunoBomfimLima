using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products;

public interface IProductRepository
{
    Task<bool> ExistsByCodeAsync(string code, Guid? excludingProductId, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);

    Task UpdateAsync(Product product, CancellationToken cancellationToken);

    Task DeleteAsync(Product product, CancellationToken cancellationToken);
}
