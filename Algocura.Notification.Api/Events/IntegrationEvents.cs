namespace Algocura.Notification.Api.Events;

public abstract record IntegrationEvent(Guid EventId, DateTimeOffset OccurredAtUtc)
{
    public abstract string EventType { get; }
    public virtual string EventVersion => "1.0";
    public virtual string Source => "Algocura.Notification.Api";
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
