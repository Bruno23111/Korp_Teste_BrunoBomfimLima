using BillingService.Application.Authentication;
using BillingService.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace BillingService.Infrastructure.Persistence.Repositories;

public sealed class UserRepository(BillingDbContext context) : IUserRepository
{
    public Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken) =>
        context.Users.SingleOrDefaultAsync(user => user.Username == username.Trim().ToLower(), cancellationToken);

    public async Task AddAsync(User user, CancellationToken cancellationToken)
    {
        context.Users.Add(user);
        await context.SaveChangesAsync(cancellationToken);
    }
}
