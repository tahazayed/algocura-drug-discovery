namespace Algocura.Notification.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : Algocura.Platform.Messaging.Events.IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.Notification.Api";
}

public sealed record NotificationDelivered(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid NotificationId,
    Guid SourceEventId, string SourceEventType, int DeliveredCount)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(NotificationDelivered);
}

public sealed record NotificationDeliveryFailed(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid NotificationId,
    Guid SourceEventId, string SourceEventType, int FailedCount)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(NotificationDeliveryFailed);
}

public sealed record NotificationSnapshot(
    Guid EventId, DateTimeOffset OccurredAtUtc, Guid NotificationId,
    Guid SourceEventId, string SourceEventType, string SourceService,
    string Subject, string Status, string Priority,
    int DeliveredCount, int FailedCount)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType => nameof(NotificationSnapshot);
}
