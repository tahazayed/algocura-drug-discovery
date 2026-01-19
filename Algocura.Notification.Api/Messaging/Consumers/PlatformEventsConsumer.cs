using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Services;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Notification.Api.Messaging.Consumers;

public sealed class PlatformEventsConsumer
    : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<PlatformEventsConsumer> _logger;

    public PlatformEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> serviceBusOptions,
        IConfiguration configuration,
        ILogger<PlatformEventsConsumer> logger)
    {
        _scopeFactory =
            scopeFactory;

        _logger =
            logger;

        var messagingOptions =
            serviceBusOptions.Value;

        var subscriptionName =
            configuration[
                "ServiceBus:SubscriptionName"];

        if (string.IsNullOrWhiteSpace(
                subscriptionName))
        {
            throw new InvalidOperationException(
                "ServiceBus:SubscriptionName is required.");
        }

        _processor =
            client.CreateProcessor(
                messagingOptions.TopicName,
                subscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages =
                        false,

                    MaxConcurrentCalls =
                        4,

                    MaxAutoLockRenewalDuration =
                        TimeSpan.FromMinutes(5)
                });
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync +=
            ProcessMessageAsync;

        _processor.ProcessErrorAsync +=
            ProcessErrorAsync;

        await _processor.StartProcessingAsync(
            stoppingToken);

        try
        {
            await Task.Delay(
                Timeout.InfiniteTimeSpan,
                stoppingToken);
        }
        catch (OperationCanceledException)
            when (stoppingToken.IsCancellationRequested)
        {
        }
    }

    private async Task ProcessMessageAsync(
        ProcessMessageEventArgs args)
    {
        try
        {
            var eventType =
                args.Message.ApplicationProperties
                    .TryGetValue(
                        "eventType",
                        out var eventTypeValue)
                    ? eventTypeValue?.ToString()
                    : args.Message.Subject;

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

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error. Entity: {Entity}; Source: {Source}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(
            cancellationToken);

        await base.StopAsync(
            cancellationToken);
    }

    public override void Dispose()
    {
        _processor.DisposeAsync()
            .AsTask()
            .GetAwaiter()
            .GetResult();

        base.Dispose();
    }
}