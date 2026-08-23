namespace BillingService.Application.Authentication;

public sealed record AuthResponse(string AccessToken, DateTimeOffset ExpiresAt, string Username, string Role);
