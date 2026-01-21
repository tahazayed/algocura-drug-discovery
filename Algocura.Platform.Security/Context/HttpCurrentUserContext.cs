using Algocura.Platform.Security.Extensions;
using Microsoft.AspNetCore.Http;

namespace Algocura.Platform.Security.Context;

public sealed class HttpCurrentUserContext(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext
{
    private HttpContext? HttpContext =>
        httpContextAccessor.HttpContext;

    public bool IsAuthenticated =>
        HttpContext?
            .User
            .Identity?
            .IsAuthenticated
        == true;

    public Guid? UserId =>
        HttpContext?
            .User
            .GetUserId();

    public string? DisplayName =>
        HttpContext?
            .User
            .GetDisplayName();

    public string? Email =>
        HttpContext?
            .User
            .GetEmail();

    public IReadOnlyCollection<string> Roles =>
        HttpContext is null
            ? Array.Empty<string>()
            : HttpContext.User.GetRoles();

    public bool IsInRole(
        string role)
    {
        if (string.IsNullOrWhiteSpace(
                role))
        {
            return false;
        }

        return HttpContext?
            .User
            .IsInRole(
                role)
            == true;
    }
}
