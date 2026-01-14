using Algocura.Notification.Api.Domain;

namespace Algocura.Notification.Api.Services;

public sealed record PlatformEventEnvelope(
    Guid EventId,
    string EventType,
    string Source,
    string? CorrelationId,
    DateTimeOffset OccurredAtUtc,
    string RawPayload);

public interface INotificationComposer
{
    Algocura.Notification.Api.Domain.Notification Compose(
        NotificationRule rule,
        PlatformEventEnvelope platformEvent);
}

public sealed class DefaultNotificationComposer : INotificationComposer
{
    public Algocura.Notification.Api.Domain.Notification Compose(
        NotificationRule rule,
        PlatformEventEnvelope platformEvent)
    {
        var now = DateTimeOffset.UtcNow;

        var notification = new Algocura.Notification.Api.Domain.Notification
        {
            Id = Guid.NewGuid(),
            RuleId = rule.Id,
            SourceEventId = platformEvent.EventId,
            SourceEventType = platformEvent.EventType,
            SourceService = platformEvent.Source,
            Subject = ReplaceTokens(rule.SubjectTemplate, platformEvent),
            Body = ReplaceTokens(rule.BodyTemplate, platformEvent),
            Priority = rule.Priority,
            Status = NotificationStatus.Pending,
            CorrelationId = platformEvent.CorrelationId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        notification.Deliveries.Add(
            new NotificationDelivery
            {
                Id = Guid.NewGuid(),
                NotificationId = notification.Id,
                Channel = rule.Channel,
                Recipient = rule.Recipient,
                Status = DeliveryStatus.Pending,
                AttemptCount = 0,
                CreatedAtUtc = now,
                UpdatedAtUtc = now
            });

        return notification;
    }

    private static string ReplaceTokens(
        string template,
        PlatformEventEnvelope platformEvent) =>
        template
            .Replace("{{eventId}}", platformEvent.EventId.ToString(),
                StringComparison.OrdinalIgnoreCase)
            .Replace("{{eventType}}", platformEvent.EventType,
                StringComparison.OrdinalIgnoreCase)
            .Replace("{{source}}", platformEvent.Source,
                StringComparison.OrdinalIgnoreCase)
            .Replace("{{correlationId}}", platformEvent.CorrelationId ?? string.Empty,
                StringComparison.OrdinalIgnoreCase)
            .Replace("{{occurredAtUtc}}", platformEvent.OccurredAtUtc.ToString("O"),
                StringComparison.OrdinalIgnoreCase);
}

public sealed record NotificationDispatchResult(
    bool Succeeded,
    string? ProviderMessageId,
    string? ErrorMessage);

public interface INotificationDispatcher
{
    Task<NotificationDispatchResult> DispatchAsync(
        Algocura.Notification.Api.Domain.Notification notification,
        NotificationDelivery delivery,
        CancellationToken cancellationToken = default);
}

public sealed class LoggingNotificationDispatcher(
    ILogger<LoggingNotificationDispatcher> logger) : INotificationDispatcher
{
    public Task<NotificationDispatchResult> DispatchAsync(
        Algocura.Notification.Api.Domain.Notification notification,
        NotificationDelivery delivery,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        logger.LogInformation(
            "Prototype notification delivery. Channel: {Channel}; Recipient: {Recipient}; Subject: {Subject}.",
            delivery.Channel,
            delivery.Recipient,
            notification.Subject);

        return Task.FromResult(
            new NotificationDispatchResult(
                true,
                $"prototype-{Guid.NewGuid():N}",
                null));
    }
}
