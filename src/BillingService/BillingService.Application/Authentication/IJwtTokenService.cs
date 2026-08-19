using BillingService.Domain.Entities;

namespace BillingService.Application.Authentication;

public interface IJwtTokenService
{
    AuthResponse Create(User user);
}
