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
}
