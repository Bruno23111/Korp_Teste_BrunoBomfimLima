using BillingService.Application.Authentication;
using BillingService.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace BillingService.Infrastructure.Authentication;

public sealed class PasswordService(IPasswordHasher<User> passwordHasher) : IPasswordService
{
    public string Hash(User user, string password) => passwordHasher.HashPassword(user, password);

    public bool Verify(User user, string password, string passwordHash) =>
        passwordHasher.VerifyHashedPassword(user, passwordHash, password) is PasswordVerificationResult.Success or PasswordVerificationResult.SuccessRehashNeeded;
}
