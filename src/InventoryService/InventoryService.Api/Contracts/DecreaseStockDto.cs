using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.Contracts;

public sealed class DecreaseStockDto
{
    [Required]
    [StringLength(100)]
    public string OperationKey { get; init; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<DecreaseStockItemDto> Items { get; init; } = [];
}
