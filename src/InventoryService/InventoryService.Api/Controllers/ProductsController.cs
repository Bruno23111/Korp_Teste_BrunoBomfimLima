using InventoryService.Api.Contracts;
using InventoryService.Application.Products;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/products")]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Create(
        CreateProductDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.CreateAsync(
                new CreateProductRequest(request.Code, request.Description, request.AvailableQuantity),
                cancellationToken);

            var response = ProductDto.From(product);

            return CreatedAtAction(nameof(GetById), new { id = response.Id }, response);
        }
        catch (ProductCodeAlreadyExistsException exception)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Product code already registered.",
                Detail = exception.Message,
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<ProductDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<ProductDto>>> GetAll(CancellationToken cancellationToken)
    {
        var products = await productService.GetAllAsync(cancellationToken);

        return Ok(products.Select(ProductDto.From).ToList());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ProductDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var product = await productService.GetByIdAsync(id, cancellationToken);

        return product is null ? NotFound() : Ok(ProductDto.From(product));
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductDto request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.UpdateAsync(
                new UpdateProductRequest(id, request.Code, request.Description, request.AvailableQuantity),
                cancellationToken);

            return product is null ? NotFound() : Ok(ProductDto.From(product));
        }
        catch (ProductCodeAlreadyExistsException exception)
        {
            return Conflict(new ProblemDetails { Title = "Product code already registered.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (ProductHasInvoicesException exception)
        {
            return Conflict(new ProblemDetails { Title = "Product cannot be changed.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await productService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (ProductHasInvoicesException exception)
        {
            return Conflict(new ProblemDetails { Title = "Product cannot be deleted.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
