using Algocura.Identity.Api.Domain;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace Algocura.Identity.Api.Security;

public sealed class JwtTokenService(
    UserManager<ApplicationUser> userManager,
    IOptions<JwtOptions> options)
    : IJwtTokenService
{
    public async Task<(string Token, DateTimeOffset ExpiresAtUtc)>
        CreateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default)
    {
        var jwtOptions =
            options.Value;

        var roles =
            await userManager.GetRolesAsync(user);

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new(
                    JwtRegisteredClaimNames.Email,
                    user.Email ?? string.Empty),

                new(
                    ClaimTypes.NameIdentifier,
                    user.Id.ToString()),

                new(
                    ClaimTypes.Name,
                    user.DisplayName ??
                    user.Email ??
                    user.Id.ToString())
            };

        claims.AddRange(
            roles.Select(role =>
                new Claim(
                    ClaimTypes.Role,
                    role)));

        var expiresAtUtc =
            DateTimeOffset.UtcNow.AddMinutes(
                jwtOptions.AccessTokenMinutes);

        var key =
            new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(
                    jwtOptions.SigningKey));

        var credentials =
            new SigningCredentials(
                key,
                SecurityAlgorithms.HmacSha256);

        var token =
            new JwtSecurityToken(
                issuer:
                    jwtOptions.Issuer,
                audience:
                    jwtOptions.Audience,
                claims:
                    claims,
                notBefore:
                    DateTime.UtcNow,
                expires:
                    expiresAtUtc.UtcDateTime,
                signingCredentials:
                    credentials);

        return (
            new JwtSecurityTokenHandler()
                .WriteToken(token),
            expiresAtUtc);
    }
}
