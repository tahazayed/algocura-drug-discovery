using Algocura.Identity.Api.Domain;

namespace Algocura.Identity.Api.Security;

public interface IJwtTokenService
{
    Task<(string Token, DateTimeOffset ExpiresAtUtc)>
        CreateAsync(
            ApplicationUser user,
            CancellationToken cancellationToken = default);
}
