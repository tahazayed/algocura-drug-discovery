namespace Algocura.Platform.Messaging.Events;

public interface IIntegrationEvent
{
    Guid EventId { get; }

    DateTimeOffset OccurredAtUtc { get; }

    string EventType { get; }

    string EventVersion { get; }

    string Source { get; }
}
