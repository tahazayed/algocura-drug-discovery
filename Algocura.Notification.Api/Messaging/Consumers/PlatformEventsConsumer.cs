using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Services;
using Algocura.Platform.Messaging.Consumers;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Notification.Api.Messaging.Consumers;

public sealed class PlatformEventsConsumer
    : ServiceBusConsumerBase
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<PlatformEventsConsumer> _logger;

    public PlatformEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        ILogger<PlatformEventsConsumer> logger)
        : base(
            client,
            serviceBusOptions,
            logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;
    }

    protected override async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            var eventType =
                ResolveEventType(
                    args.Message);

            if (string.IsNullOrWhiteSpace(
                    eventType))
            {
                throw new JsonException(
                    "The event type is missing.");
            }

            var source =
                args.Message.ApplicationProperties
                    .TryGetValue(
                        "source",
                        out var sourceValue)
                    ? sourceValue?.ToString() ??
                      "Unknown"
                    : "Unknown";

            var eventId =
                Guid.TryParse(
                    args.Message.MessageId,
                    out var parsed)
                    ? parsed
                    : Guid.NewGuid();

            var envelope =
                new PlatformEventEnvelope(
                    eventId,
                    eventType,
                    source,
                    args.Message.CorrelationId,
                    DateTimeOffset.UtcNow,
                    args.Message.Body.ToString());

            await using var scope =
                _scopeFactory.CreateAsyncScope();

            var dbContext =
                scope.ServiceProvider
                    .GetRequiredService<
                        NotificationDbContext>();

            var composer =
                scope.ServiceProvider
                    .GetRequiredService<
                        INotificationComposer>();

            await HandleAsync(
                dbContext,
                composer,
                envelope,
                args.CancellationToken);

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);
        }
        catch (JsonException exception)
        {
            await args.DeadLetterMessageAsync(
                args.Message,
                "InvalidPayload",
                exception.Message,
                args.CancellationToken);
        }
        catch (Exception exception)
        {
            _logger.LogError(
                exception,
                "Failed to process message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken:
                    args.CancellationToken);
        }
    }

    public static async Task<int> HandleAsync(
        NotificationDbContext dbContext,
        INotificationComposer composer,
        PlatformEventEnvelope platformEvent,
        CancellationToken cancellationToken)
    {
        var rules =
            await dbContext.NotificationRules
                .AsNoTracking()
                .Where(
                    x =>
                        x.Enabled &&
                        x.EventType ==
                        platformEvent.EventType)
                .ToListAsync(
                    cancellationToken);

        if (rules.Count == 0)
        {
            return 0;
        }

        var existingRuleIds =
            await dbContext.Notifications
                .Where(
                    x =>
                        x.SourceEventId ==
                            platformEvent.EventId &&
                        x.RuleId != null)
                .Select(
                    x =>
                        x.RuleId!.Value)
                .ToListAsync(
                    cancellationToken);

        var existing =
            existingRuleIds.ToHashSet();

        var notifications =
            rules
                .Where(
                    rule =>
                        !existing.Contains(
                            rule.Id))
                .Select(
                    rule =>
                        composer.Compose(
                            rule,
                            platformEvent))
                .ToArray();

        if (notifications.Length == 0)
        {
            return 0;
        }

        dbContext.Notifications.AddRange(
            notifications);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return notifications.Length;
    }
}
