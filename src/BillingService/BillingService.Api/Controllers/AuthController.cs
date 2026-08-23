using BillingService.Api.Contracts;
using BillingService.Application.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BillingService.Api.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(IAuthService authService) : ControllerBase
{
    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<ActionResult<AuthResponseDto>> Login(LoginDto request, CancellationToken cancellationToken)
    {
        var response = await authService.LoginAsync(new LoginRequest(request.Username, request.Password), cancellationToken);
        return response is null
            ? Unauthorized(new ProblemDetails { Title = "Invalid credentials.", Status = StatusCodes.Status401Unauthorized })
            : Ok(AuthResponseDto.From(response));
    }
}
