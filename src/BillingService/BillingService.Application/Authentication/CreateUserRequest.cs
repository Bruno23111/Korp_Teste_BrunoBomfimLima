namespace BillingService.Application.Authentication;

public sealed record CreateUserRequest(string Username, string Password, string Role);
