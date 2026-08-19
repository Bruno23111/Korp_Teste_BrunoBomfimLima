using BillingService.Domain.Entities;

namespace BillingService.Application.Authentication;

public interface IPasswordService
{
    string Hash(User user, string password);
    bool Verify(User user, string password, string passwordHash);
}
