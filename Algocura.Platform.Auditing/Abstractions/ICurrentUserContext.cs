namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Provides identity and request metadata for the current authenticated request.
/// </summary>
public interface ICurrentUserContext
{
    Guid? UserId { get; }

    string? Subject { get; }

    string? DisplayName { get; }

    string? Email { get; }

    string ActorType { get; }

    string? CorrelationId { get; }

    string? TraceId { get; }

    string? IpAddress { get; }

    string? UserAgent { get; }

    bool IsAuthenticated { get; }
}

