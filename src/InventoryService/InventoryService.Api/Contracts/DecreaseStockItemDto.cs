using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.Contracts;

public sealed class DecreaseStockItemDto
{
    public Guid ProductId { get; init; }

    [Range(
        typeof(decimal),
        "0.0001",
        "79228162514264337593543950335",
        ParseLimitsInInvariantCulture = true,
        ConvertValueInInvariantCulture = true)]
    public decimal Quantity { get; init; }
}
