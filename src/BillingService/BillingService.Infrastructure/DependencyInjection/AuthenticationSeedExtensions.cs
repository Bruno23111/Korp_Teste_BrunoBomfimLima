using BillingService.Domain.Entities;
using BillingService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace BillingService.Infrastructure.DependencyInjection;

public static class AuthenticationSeedExtensions
{
    public static async Task EnsureDefaultUserAsync(this IServiceProvider services, IConfiguration configuration)
    {
        var username = configuration["Authentication:SeedUser:Username"];
        var password = configuration["Authentication:SeedUser:Password"];
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password)) return;

        await using var scope = services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<BillingDbContext>();
        await context.Database.MigrateAsync();
        if (await context.Users.AnyAsync(user => user.Username == username.Trim().ToLower())) return;

        var user = new User(username, "temporary", "Admin", DateTimeOffset.UtcNow);
        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher<User>>();
        context.Users.Add(new User(username, hasher.HashPassword(user, password), "Admin", DateTimeOffset.UtcNow));
        await context.SaveChangesAsync();
    }
}
