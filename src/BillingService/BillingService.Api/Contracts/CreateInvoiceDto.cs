using System.ComponentModel.DataAnnotations;

namespace BillingService.Api.Contracts;

public sealed class CreateInvoiceDto
{
    [Required]
    [MinLength(1)]
    public IReadOnlyList<CreateInvoiceItemDto> Items { get; init; } = [];
}
