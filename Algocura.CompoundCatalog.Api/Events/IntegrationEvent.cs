namespace Algocura.CompoundCatalog.Api.Events;

public abstract record IntegrationEvent
{
    public Guid EventId { get; init; } = Guid.NewGuid();

    public DateTimeOffset OccurredAtUtc { get; init; } =
        DateTimeOffset.UtcNow;

    public string EventType => GetType().Name;

    public string EventVersion { get; init; } = "1.0";

    public string Source { get; init; } =
        "algocura.compound-catalog";
}