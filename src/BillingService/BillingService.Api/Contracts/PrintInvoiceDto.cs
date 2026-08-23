using System.ComponentModel.DataAnnotations;

namespace BillingService.Api.Contracts;

public sealed class PrintInvoiceDto
{
    [Required]
    [StringLength(100)]
    public string IdempotencyKey { get; init; } = string.Empty;
}
