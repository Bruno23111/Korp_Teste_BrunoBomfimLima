using BillingService.Api.Contracts;
using BillingService.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/users")]
[Authorize(Roles = "Admin")]
public sealed class UsersController(IUserService userService) : ControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
    public async Task<ActionResult<UserResponse>> Create(CreateUserDto request, CancellationToken cancellationToken)
    {
        try
        {
            var user = await userService.CreateAsync(
                new CreateUserRequest(request.Username, request.Password, request.Role),
                cancellationToken);
            return Created($"api/users/{user.Id}", user);
        }
        catch (ArgumentException exception)
        {
            return BadRequest(new ProblemDetails { Title = "Invalid user data.", Detail = exception.Message, Status = StatusCodes.Status400BadRequest });
        }
        catch (InvalidOperationException exception)
        {
            return Conflict(new ProblemDetails { Title = "Username already registered.", Detail = exception.Message, Status = StatusCodes.Status409Conflict });
        }
    }
}
