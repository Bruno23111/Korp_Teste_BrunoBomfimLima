using InventoryService.Api.Contracts;
using InventoryService.Application.Products;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/products")]
[Authorize]
public sealed class ProductsController(IProductService productService) : ControllerBase
{
    [HttpPost]
    [Authorize(Roles = "Admin")]
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
                new CreateProductRequest(request.Code, request.Description, request.AvailableQuantity, request.UnitPrice),
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
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(ProductDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<ProductDto>> Update(Guid id, UpdateProductDto request, CancellationToken cancellationToken)
    {
        try
        {
            var product = await productService.UpdateAsync(
                new UpdateProductRequest(id, request.Code, request.Description, request.AvailableQuantity, request.UnitPrice),
                cancellationToken);

            return product is null ? NotFound() : Ok(ProductDto.From(product));
        }
        catch (ProductCodeAlreadyExistsException exception)
        {
            return Conflict(new ProblemDetails { Title = "Product code already registered.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
        catch (ProductHasInvoicesException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não é possível alterar o produto.",
                Detail = "Este produto está vinculado a uma nota fiscal e não pode ser alterado.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        try
        {
            return await productService.DeleteAsync(id, cancellationToken) ? NoContent() : NotFound();
        }
        catch (ProductHasInvoicesException)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Não é possível excluir o produto.",
                Detail = "Este produto está vinculado a uma nota fiscal e não pode ser excluído.",
                Status = StatusCodes.Status409Conflict
            });
        }
    }
}
