namespace BillingService.Api.Contracts;

public sealed record CreateUserDto(string Username, string Password, string Role);
