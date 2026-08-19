namespace BillingService.Application.Authentication;

public sealed class AuthService(
    IUserRepository userRepository,
    IPasswordService passwordService,
    IJwtTokenService jwtTokenService) : IAuthService
{
    public async Task<AuthResponse?> LoginAsync(LoginRequest request, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (user is null || !user.IsActive || !passwordService.Verify(user, request.Password, user.PasswordHash))
        {
            return null;
        }

        return jwtTokenService.Create(user);
    }
}
