using Algocura.Platform.Security.Claims;
using System.Security.Claims;

namespace Algocura.Platform.Security.Extensions;

public static class ClaimsPrincipalExtensions
{
    public static Guid? GetUserId(
        this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(
            principal);

        var value =
            principal.FindFirstValue(
                ClaimTypes.NameIdentifier)
            ?? principal.FindFirstValue(
                PlatformClaimTypes.Subject);

        return Guid.TryParse(
            value,
            out var userId)
            ? userId
            : null;
    }

    public static string? GetDisplayName(
        this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(
            principal);

        return principal.FindFirstValue(
            ClaimTypes.Name);
    }

    public static string? GetEmail(
        this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(
            principal);

        return principal.FindFirstValue(
                   ClaimTypes.Email)
            ?? principal.FindFirstValue(
                PlatformClaimTypes.Email);
    }

    public static IReadOnlyCollection<string> GetRoles(
        this ClaimsPrincipal principal)
    {
        ArgumentNullException.ThrowIfNull(
            principal);

        return principal.FindAll(
                ClaimTypes.Role)
            .Select(
                claim => claim.Value)
            .Where(
                value =>
                    !string.IsNullOrWhiteSpace(
                        value))
            .Distinct(
                StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
