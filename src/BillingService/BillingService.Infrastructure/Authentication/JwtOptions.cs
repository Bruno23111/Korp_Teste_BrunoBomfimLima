namespace BillingService.Infrastructure.Authentication;

public sealed class JwtOptions
{
    public string Issuer { get; init; } = string.Empty;
    public string Audience { get; init; } = string.Empty;
    public string Secret { get; init; } = string.Empty;
    public int ExpirationMinutes { get; init; } = 15;

    public void Validate()
    {
        if (string.IsNullOrWhiteSpace(Issuer)) throw new InvalidOperationException("JWT issuer is not configured.");
        if (string.IsNullOrWhiteSpace(Audience)) throw new InvalidOperationException("JWT audience is not configured.");
        if (Secret.Length < 32) throw new InvalidOperationException("JWT secret must contain at least 32 characters.");
    }
}
