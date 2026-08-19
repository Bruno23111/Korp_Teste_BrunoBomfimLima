using BillingService.Application.Authentication;

namespace BillingService.Api.Contracts;

public sealed record AuthResponseDto(string AccessToken, DateTimeOffset ExpiresAt, string Username, string Role)
{
    public static AuthResponseDto From(AuthResponse response) =>
        new(response.AccessToken, response.ExpiresAt, response.Username, response.Role);
}
