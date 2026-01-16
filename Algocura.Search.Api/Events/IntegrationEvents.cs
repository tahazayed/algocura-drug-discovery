namespace Algocura.Search.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
{
    public abstract string EventType { get; }
    public virtual string EventVersion => "1.0";
    public virtual string Source => "Algocura.Search.Api";
}

public sealed record SearchDocumentIndexed(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid SearchDocumentId,
    Guid SourceEntityId,
    string DocumentType,
    string SourceEventType)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(SearchDocumentIndexed);
}

public sealed record SearchIndexSnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    int ActiveDocumentCount,
    IReadOnlyDictionary<string, int> DocumentTypeCounts)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(SearchIndexSnapshot);
}

