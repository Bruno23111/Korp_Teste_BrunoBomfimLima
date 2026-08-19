using BillingService.Domain.Entities;

namespace BillingService.Application.Authentication;

public sealed class UserService(
    IUserRepository userRepository,
    IPasswordService passwordService) : IUserService
{
    private static readonly string[] AllowedRoles = ["Admin", "Operator"];

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken cancellationToken)
    {
        var username = request.Username?.Trim().ToLowerInvariant();
        var role = request.Role?.Trim();

        if (string.IsNullOrWhiteSpace(username) || username.Length > 100)
            throw new ArgumentException("Username is required and must contain at most 100 characters.", nameof(request));
        if (string.IsNullOrWhiteSpace(request.Password) || request.Password.Length < 8)
            throw new ArgumentException("Password must contain at least 8 characters.", nameof(request));
        var normalizedRole = AllowedRoles.FirstOrDefault(allowedRole => string.Equals(allowedRole, role, StringComparison.OrdinalIgnoreCase));
        if (normalizedRole is null)
            throw new ArgumentException("Role must be Admin or Operator.", nameof(request));
        if (await userRepository.GetByUsernameAsync(username, cancellationToken) is not null)
            throw new InvalidOperationException("Username is already registered.");

        var user = new User(username, "temporary", normalizedRole, DateTimeOffset.UtcNow);
        var passwordHash = passwordService.Hash(user, request.Password);
        var persistedUser = new User(username, passwordHash, normalizedRole, user.CreatedAt);
        await userRepository.AddAsync(persistedUser, cancellationToken);
        return UserResponse.From(persistedUser);
    }
}
