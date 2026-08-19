using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BillingService.Application.Authentication;
using BillingService.Domain.Entities;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace BillingService.Infrastructure.Authentication;

public sealed class JwtTokenService(IOptions<JwtOptions> options) : IJwtTokenService
{
    public AuthResponse Create(User user)
    {
        var jwt = options.Value;
        jwt.Validate();
        var expiresAt = DateTimeOffset.UtcNow.AddMinutes(jwt.ExpirationMinutes);
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
            new Claim(ClaimTypes.Name, user.Username),
            new Claim(ClaimTypes.Role, user.Role),
        };
        var credentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwt.Secret)), SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(jwt.Issuer, jwt.Audience, claims, expires: expiresAt.UtcDateTime, signingCredentials: credentials);
        return new AuthResponse(new JwtSecurityTokenHandler().WriteToken(token), expiresAt, user.Username, user.Role);
    }
}
