using InventoryService.Application.Products;
using InventoryService.Domain.Entities;

namespace InventoryService.Application.Stock;

public sealed class StockService(
    IProductRepository productRepository,
    IStockMovementRepository stockMovementRepository,
    IInventoryUnitOfWork unitOfWork) : IStockService
{
    public async Task<DecreaseStockResponse> DecreaseAsync(
        DecreaseStockRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        await using var transaction = await unitOfWork.BeginTransactionAsync(cancellationToken);

        try
        {
            var existingMovements = await stockMovementRepository.GetByOperationKeyAsync(
                request.OperationKey,
                cancellationToken);

            if (existingMovements.Count > 0)
            {
                EnsureSameOperation(existingMovements, request.Items);
                return new DecreaseStockResponse(request.OperationKey, true);
            }

            var productIds = request.Items.Select(item => item.ProductId).ToHashSet();
            var products = await productRepository.GetByIdsForUpdateAsync(productIds, cancellationToken);
            var productsById = products.ToDictionary(product => product.Id);
            var missingProductId = productIds.FirstOrDefault(productId => !productsById.ContainsKey(productId));

            if (missingProductId != Guid.Empty)
            {
                throw new ProductNotFoundException(missingProductId);
            }

            var occurredAt = DateTimeOffset.UtcNow;
            var movements = request.Items
                .Select(item => productsById[item.ProductId].DecreaseStock(item.Quantity, request.OperationKey, occurredAt))
                .ToList();

            stockMovementRepository.AddRange(movements);
            await unitOfWork.SaveChangesAsync(cancellationToken);
            await transaction.CommitAsync(cancellationToken);

            return new DecreaseStockResponse(request.OperationKey, false);
        }
        catch
        {
            unitOfWork.ClearChanges();
            throw;
        }
    }

    private static void ValidateRequest(DecreaseStockRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.OperationKey))
        {
            throw new ArgumentException("Operation key is required.", nameof(request));
        }

        if (request.Items.Count == 0)
        {
            throw new ArgumentException("At least one stock item is required.", nameof(request));
        }

        if (request.Items.Any(item => item.ProductId == Guid.Empty || item.Quantity <= 0))
        {
            throw new ArgumentException("Each stock item must contain a product identifier and a positive quantity.", nameof(request));
        }

        if (request.Items.Select(item => item.ProductId).Distinct().Count() != request.Items.Count)
        {
            throw new ArgumentException("A product can only appear once in a stock operation.", nameof(request));
        }
    }

    private static void EnsureSameOperation(
        IReadOnlyList<StockMovement> existingMovements,
        IReadOnlyList<DecreaseStockItem> requestedItems)
    {
        var existingByProductId = existingMovements.ToDictionary(movement => movement.ProductId);

        var isSameOperation = existingByProductId.Count == requestedItems.Count
            && requestedItems.All(item =>
                existingByProductId.TryGetValue(item.ProductId, out var movement)
                && movement.Quantity == item.Quantity);

        if (!isSameOperation)
        {
            throw new StockOperationKeyConflictException(existingMovements[0].OperationKey);
        }
    }
}
