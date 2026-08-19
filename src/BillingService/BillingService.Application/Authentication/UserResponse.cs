using BillingService.Domain.Entities;

namespace BillingService.Application.Authentication;

public sealed record UserResponse(Guid Id, string Username, string Role, bool IsActive, DateTimeOffset CreatedAt)
{
    public static UserResponse From(User user) => new(user.Id, user.Username, user.Role, user.IsActive, user.CreatedAt);
}
