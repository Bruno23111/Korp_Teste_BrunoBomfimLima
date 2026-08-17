using InventoryService.Domain.Entities;

namespace InventoryService.Application.Products;

public interface IProductRepository
{
    Task<bool> ExistsByCodeAsync(string code, CancellationToken cancellationToken);

    Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken);

    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken);

    Task AddAsync(Product product, CancellationToken cancellationToken);
}
