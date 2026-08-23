using InventoryService.Application.Products;
using InventoryService.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InventoryService.Infrastructure.Persistence.Repositories;

public sealed class ProductRepository(InventoryDbContext context) : IProductRepository
{
    public Task<bool> ExistsByCodeAsync(string code, Guid? excludingProductId, CancellationToken cancellationToken) =>
        context.Products
            .AsNoTracking()
            .AnyAsync(product => product.Code == code && product.Id != excludingProductId, cancellationToken);

    public Task<Product?> GetByIdAsync(Guid id, CancellationToken cancellationToken) =>
        context.Products
            .AsNoTracking()
            .SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public Task<Product?> GetByIdForUpdateAsync(Guid id, CancellationToken cancellationToken) =>
        context.Products.SingleOrDefaultAsync(product => product.Id == id, cancellationToken);

    public async Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken cancellationToken)
    {
        return await context.Products
            .AsNoTracking()
            .OrderBy(product => product.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Product>> GetByIdsForUpdateAsync(
        IReadOnlyCollection<Guid> productIds,
        CancellationToken cancellationToken)
    {
        return await context.Products
            .Where(product => productIds.Contains(product.Id))
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Add(product);

        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ProductCodeAlreadyExistsException(product.Code);
        }
    }

    public Task UpdateAsync(Product product, CancellationToken cancellationToken) => SaveChangesAsync(product.Code, cancellationToken);

    public async Task DeleteAsync(Product product, CancellationToken cancellationToken)
    {
        context.Products.Remove(product);
        await context.SaveChangesAsync(cancellationToken);
    }

    private async Task SaveChangesAsync(string productCode, CancellationToken cancellationToken)
    {
        try
        {
            await context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (
            exception.InnerException is PostgresException { SqlState: PostgresErrorCodes.UniqueViolation })
        {
            throw new ProductCodeAlreadyExistsException(productCode);
        }
    }
}
