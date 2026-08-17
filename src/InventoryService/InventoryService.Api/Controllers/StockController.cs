using InventoryService.Api.Contracts;
using InventoryService.Application.Stock;
using InventoryService.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace InventoryService.Api.Controllers;

[ApiController]
[Route("api/stock")]
public sealed class StockController(IStockService stockService) : ControllerBase
{
    [HttpPost("decreases")]
    [ProducesResponseType(typeof(DecreaseStockResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<DecreaseStockResponseDto>> Decrease(
        DecreaseStockDto request,
        CancellationToken cancellationToken)
    {
        try
        {
            var result = await stockService.DecreaseAsync(
                new DecreaseStockRequest(
                    request.OperationKey,
                    request.Items.Select(item => new DecreaseStockItem(item.ProductId, item.Quantity)).ToList()),
                cancellationToken);

            return Ok(DecreaseStockResponseDto.From(result));
        }
        catch (ProductNotFoundException exception)
        {
            return NotFound(CreateProblemDetails(exception.Message, StatusCodes.Status404NotFound));
        }
        catch (InsufficientStockException exception)
        {
            return Conflict(CreateProblemDetails(exception.Message, StatusCodes.Status409Conflict));
        }
        catch (StockOperationKeyConflictException exception)
        {
            return Conflict(CreateProblemDetails(exception.Message, StatusCodes.Status409Conflict));
        }
        catch (StockConcurrencyException exception)
        {
            return Conflict(CreateProblemDetails(exception.Message, StatusCodes.Status409Conflict));
        }
    }

    private static ProblemDetails CreateProblemDetails(string detail, int status) => new()
    {
        Title = "Stock operation could not be completed.",
        Detail = detail,
        Status = status
    };
}
