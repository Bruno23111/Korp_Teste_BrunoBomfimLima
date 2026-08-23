using BillingService.Api.Contracts;
using BillingService.Application.Invoices;
using BillingService.Application.Printing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/invoices")]
[Authorize]
public sealed class InvoicesController(IInvoiceService invoiceService, IPrintInvoiceService printInvoiceService) : ControllerBase
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
        catch (InsufficientInvoiceStockException exception)
        {
            return Conflict(new ProblemDetails { Title = "Quantidade de produto insuficiente.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
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

    [HttpGet("products/{productId:guid}/exists")]
    public async Task<ActionResult<bool>> HasProduct(Guid productId, CancellationToken cancellationToken) =>
        Ok(await invoiceService.HasProductAsync(productId, cancellationToken));

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<InvoiceDto>> Cancel(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            var invoice = await invoiceService.CancelAsync(id, cancellationToken);
            return invoice is null ? NotFound() : Ok(InvoiceDto.From(invoice));
        }
        catch (InvalidOperationException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invoice cannot be cancelled.",
                Detail = "Only open invoices can be cancelled.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpPost("{id:guid}/print")]
    public async Task<ActionResult<PrintInvoiceResponseDto>> Print(Guid id, PrintInvoiceDto request, CancellationToken cancellationToken)
    {
        try
        {
            var response = await printInvoiceService.PrintAsync(new PrintInvoiceRequest(id, request.IdempotencyKey), cancellationToken);
            return Ok(PrintInvoiceResponseDto.From(response));
        }
        catch (InvoiceNotFoundException) { return NotFound(); }
        catch (PrintInvoiceFailedException exception)
        {
            var status = exception.ErrorCode == "inventory-stock-rejected"
                ? StatusCodes.Status409Conflict
                : StatusCodes.Status503ServiceUnavailable;
            var title = status == StatusCodes.Status409Conflict
                ? "Inventory rejected the stock decrease."
                : "Inventory service unavailable.";
            return StatusCode(status, new ProblemDetails { Title = title, Detail = exception.ErrorCode, Status = status });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Invoice cannot be printed.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
