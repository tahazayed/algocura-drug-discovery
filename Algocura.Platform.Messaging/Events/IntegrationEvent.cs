namespace Algocura.Platform.Messaging.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : IIntegrationEvent
{
    public abstract string EventType { get; }

    public virtual string EventVersion =>
        "1.0";

    public abstract string Source { get; }
}
