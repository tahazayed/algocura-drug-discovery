namespace Algocura.Audit.Api.Messaging;

/// <summary>
/// Wire contract matching Algocura.Platform.Auditing.Events.EntityAuditEvent.
/// A local contract keeps the Audit API decoupled from implementation details
/// while preserving the shared event schema.
/// </summary>
public sealed record IncomingEntityAuditEvent
{
    public Guid EventId { get; init; }

    public DateTimeOffset OccurredAtUtc { get; init; }

    public string ServiceName { get; init; } =
        string.Empty;

    public string EntityType { get; init; } =
        string.Empty;

    public string EntityId { get; init; } =
        string.Empty;

    public string Action { get; init; } =
        string.Empty;

    public Guid? UserId { get; init; }

    public string? Subject { get; init; }

    public string? UserDisplayName { get; init; }

    public string? UserEmail { get; init; }

    public string ActorType { get; init; } =
        string.Empty;

    public string? PreviousValuesJson { get; init; }

    public string? CurrentValuesJson { get; init; }

    public string? ChangedPropertiesJson { get; init; }

    public string? CorrelationId { get; init; }

    public string? WorkflowId { get; init; }

    public string? TraceId { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string EventType { get; init; } =
        "EntityAuditEvent";

    public string EventVersion { get; init; } =
        "1.0";
}
