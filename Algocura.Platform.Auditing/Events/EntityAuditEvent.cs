namespace Algocura.Platform.Auditing.Events;

/// <summary>
/// Immutable audit event describing one meaningful entity change.
/// </summary>
public sealed record EntityAuditEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; }

    public required string ServiceName { get; init; }

    public required string EntityType { get; init; }

    public required string EntityId { get; init; }

    public required string Action { get; init; }

    public Guid? UserId { get; init; }

    public string? Subject { get; init; }

    public string? UserDisplayName { get; init; }

    public string? UserEmail { get; init; }

    public required string ActorType { get; init; }

    public string? PreviousValuesJson { get; init; }

    public string? CurrentValuesJson { get; init; }

    public string? ChangedPropertiesJson { get; init; }

    public string? CorrelationId { get; init; }

    public string? WorkflowId { get; init; }

    public string? TraceId { get; init; }

    public string? IpAddress { get; init; }

    public string? UserAgent { get; init; }

    public string EventType => nameof(EntityAuditEvent);

    public string EventVersion => "1.0";
}
