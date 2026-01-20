namespace Algocura.Identity.Api.Contracts;

public sealed record RegisterRequest(
    string Email,
    string Password,
    string? DisplayName);

public sealed record LoginRequest(
    string Email,
    string Password);

public sealed record AssignRoleRequest(
    string RoleName);

public sealed record UserResponse(
    Guid Id,
    string Email,
    string? DisplayName,
    bool IsActive,
    DateTimeOffset? LastLoginAtUtc,
    IReadOnlyCollection<string> Roles);

public sealed record TokenResponse(
    string AccessToken,
    DateTimeOffset ExpiresAtUtc,
    UserResponse User);
