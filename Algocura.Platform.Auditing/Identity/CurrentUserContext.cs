using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Models;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace Algocura.Platform.Auditing.Identity;

public sealed class CurrentUserContext(
    IHttpContextAccessor httpContextAccessor)
    : ICurrentUserContext
{
    private HttpContext? HttpContext =>
        httpContextAccessor.HttpContext;

    private ClaimsPrincipal? User =>
        HttpContext?.User;

    public Guid? UserId
    {
        get
        {
            var value =
                User?.FindFirst("sub")?.Value
                ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value
                ?? User?.FindFirst("oid")?.Value;

            return Guid.TryParse(value, out var userId)
                ? userId
                : null;
        }
    }

    public string? Subject =>
        User?.FindFirst("sub")?.Value
        ?? User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;

    public string? DisplayName =>
        User?.FindFirst("name")?.Value
        ?? User?.FindFirst(ClaimTypes.Name)?.Value
        ?? User?.Identity?.Name;

    public string? Email =>
        User?.FindFirst("email")?.Value
        ?? User?.FindFirst(ClaimTypes.Email)?.Value
        ?? User?.FindFirst("preferred_username")?.Value;

    public string ActorType
    {
        get
        {
            if (User?.Identity?.IsAuthenticated == true)
            {
                return User.IsInRole("Administrator")
                    ? AuditActorType.Administrator
                    : AuditActorType.User;
            }

            return HttpContext is null
                ? AuditActorType.System
                : AuditActorType.Service;
        }
    }

    public string? CorrelationId =>
        ReadHeader("X-Correlation-ID")
        ?? ReadHeader("Correlation-ID");

    public string? TraceId =>
        HttpContext?.TraceIdentifier;

    public string? IpAddress =>
        HttpContext?.Connection.RemoteIpAddress?.ToString();

    public string? UserAgent =>
        ReadHeader("User-Agent");

    public bool IsAuthenticated =>
        User?.Identity?.IsAuthenticated == true;

    private string? ReadHeader(string name)
    {
        var value =
            HttpContext?.Request.Headers[name].ToString();

        return string.IsNullOrWhiteSpace(value)
            ? null
            : value;
    }
}
