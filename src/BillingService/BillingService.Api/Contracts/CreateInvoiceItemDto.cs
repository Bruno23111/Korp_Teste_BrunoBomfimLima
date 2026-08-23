using System.ComponentModel.DataAnnotations;

namespace BillingService.Api.Contracts;

public sealed class CreateInvoiceItemDto
{
    public Guid ProductId { get; init; }
    [Required, StringLength(20)] public string ProductCode { get; init; } = string.Empty;
    [Required, StringLength(100)] public string ProductDescription { get; init; } = string.Empty;
    [Range(typeof(decimal), "0.0001", "79228162514264337593543950335", ParseLimitsInInvariantCulture = true, ConvertValueInInvariantCulture = true)]
    public decimal Quantity { get; init; }
}
