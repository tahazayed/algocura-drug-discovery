namespace Algocura.Audit.Api.Contracts;

public sealed record AuditRecordResponse(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    string ServiceName,
    string EntityType,
    string EntityId,
    string Action,
    Guid? UserId,
    string? Subject,
    string? UserDisplayName,
    string? UserEmail,
    string ActorType,
    string? PreviousValuesJson,
    string? CurrentValuesJson,
    string? ChangedPropertiesJson,
    string? CorrelationId,
    string? WorkflowId,
    string? TraceId,
    string? IpAddress,
    string? UserAgent,
    string EventType,
    string EventVersion,
    DateTimeOffset ReceivedAtUtc);

public sealed record AuditSearchResponse(
    int TotalCount,
    int Page,
    int PageSize,
    IReadOnlyCollection<AuditRecordResponse> Items);

public sealed record AuditSummaryResponse(
    int TotalEvents,
    IReadOnlyDictionary<string, int> EventsByService,
    IReadOnlyDictionary<string, int> EventsByAction,
    IReadOnlyDictionary<string, int> EventsByActorType);
