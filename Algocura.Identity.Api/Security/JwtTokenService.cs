using Algocura.Identity.Api.Domain;
using Algocura.Platform.Security.Options;
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
        ArgumentNullException.ThrowIfNull(
            user);

        cancellationToken.ThrowIfCancellationRequested();

        var jwtOptions =
            options.Value;

        var now =
            DateTimeOffset.UtcNow;

        var roles =
            await userManager.GetRolesAsync(
                user);

        cancellationToken.ThrowIfCancellationRequested();

        var claims =
            new List<Claim>
            {
                new(
                    JwtRegisteredClaimNames.Jti,
                    Guid.NewGuid().ToString()),

                new(
                    JwtRegisteredClaimNames.Sub,
                    user.Id.ToString()),

                new(
                    JwtRegisteredClaimNames.Email,
                    user.Email ??
                    string.Empty),

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
            roles.Select(
                role =>
                    new Claim(
                        ClaimTypes.Role,
                        role)));

        var expiresAtUtc =
            now.AddMinutes(
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
                    now.UtcDateTime,

                expires:
                    expiresAtUtc.UtcDateTime,

                signingCredentials:
                    credentials);

        var tokenValue =
            new JwtSecurityTokenHandler()
                .WriteToken(
                    token);

        return (
            tokenValue,
            expiresAtUtc);
    }
}