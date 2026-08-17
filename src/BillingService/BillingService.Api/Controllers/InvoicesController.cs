using BillingService.Api.Contracts;
using BillingService.Application.Invoices;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/invoices")]
public sealed class InvoicesController(IInvoiceService invoiceService) : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<InvoiceDto>> Create(CreateInvoiceDto request, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoiceService.CreateAsync(new CreateInvoiceRequest(request.Items.Select(item => new CreateInvoiceItemRequest(item.ProductId, item.ProductCode, item.ProductDescription, item.Quantity)).ToList()), cancellationToken);
            var response = InvoiceDto.From(invoice);
            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (InvoiceNumberAlreadyExistsException exception)
        {
            return Conflict(new ProblemDetails { Title = "Invoice number already registered.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<InvoiceDto>>> GetAll(CancellationToken cancellationToken) =>
        Ok((await invoiceService.GetAllAsync(cancellationToken)).Select(InvoiceDto.From).ToList());

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InvoiceDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var invoice = await invoiceService.GetByIdAsync(id, cancellationToken);
        return invoice is null ? NotFound() : Ok(InvoiceDto.From(invoice));
    }
}
