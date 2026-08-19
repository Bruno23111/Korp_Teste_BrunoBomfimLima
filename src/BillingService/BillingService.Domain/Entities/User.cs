namespace BillingService.Domain.Entities;

public sealed class User
{
    private User()
    {
    }

    public User(string username, string passwordHash, string role, DateTimeOffset createdAt)
    {
        if (string.IsNullOrWhiteSpace(username)) throw new ArgumentException("Username is required.", nameof(username));
        if (string.IsNullOrWhiteSpace(passwordHash)) throw new ArgumentException("Password hash is required.", nameof(passwordHash));
        if (string.IsNullOrWhiteSpace(role)) throw new ArgumentException("Role is required.", nameof(role));

        Id = Guid.NewGuid();
        Username = username.Trim().ToLowerInvariant();
        PasswordHash = passwordHash;
        Role = role.Trim();
        CreatedAt = createdAt;
        IsActive = true;
    }

    public Guid Id { get; private set; }
    public string Username { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = string.Empty;
    public bool IsActive { get; private set; }
    public DateTimeOffset CreatedAt { get; private set; }
}
