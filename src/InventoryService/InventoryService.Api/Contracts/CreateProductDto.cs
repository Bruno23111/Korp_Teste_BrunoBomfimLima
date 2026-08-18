using System.ComponentModel.DataAnnotations;

namespace InventoryService.Api.Contracts;

public sealed class CreateProductDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal AvailableQuantity { get; init; }
}

public sealed class UpdateProductDto
{
    [Required]
    [StringLength(20)]
    public string Code { get; init; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Description { get; init; } = string.Empty;

    [Range(typeof(decimal), "0", "79228162514264337593543950335")]
    public decimal AvailableQuantity { get; init; }
}
