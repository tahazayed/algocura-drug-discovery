using Algocura.Notification.Api.Data;
using Algocura.Notification.Api.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Notification.Api.Messaging.Consumers;

public sealed class PlatformEventsConsumer : BackgroundService
{
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger<PlatformEventsConsumer> _logger;

    public PlatformEventsConsumer(
        IServiceScopeFactory scopeFactory,
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<PlatformEventsConsumer> logger)
    {
        _scopeFactory = scopeFactory;
        _logger = logger;
        _processor = client.CreateProcessor(
            options.Value.TopicName,
            options.Value.SubscriptionName,
            new ServiceBusProcessorOptions
            {
                AutoCompleteMessages = false,
                MaxConcurrentCalls = 4,
                MaxAutoLockRenewalDuration = TimeSpan.FromMinutes(5)
            });
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync += ProcessMessageAsync;
        _processor.ProcessErrorAsync += ProcessErrorAsync;
        await _processor.StartProcessingAsync(stoppingToken);

        try { await Task.Delay(Timeout.InfiniteTimeSpan, stoppingToken); }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) { }
    }

    private async Task ProcessMessageAsync(ProcessMessageEventArgs args)
    {
        try
        {
            var eventType = args.Message.ApplicationProperties
                .TryGetValue("eventType", out var eventTypeValue)
                ? eventTypeValue?.ToString()
                : args.Message.Subject;

            if (string.IsNullOrWhiteSpace(eventType))
                throw new JsonException("The event type is missing.");

            var source = args.Message.ApplicationProperties
                .TryGetValue("source", out var sourceValue)
                ? sourceValue?.ToString() ?? "Unknown"
                : "Unknown";

            var eventId = Guid.TryParse(args.Message.MessageId, out var parsed)
                ? parsed
                : Guid.NewGuid();

            var envelope = new PlatformEventEnvelope(
                eventId,
                eventType,
                source,
                args.Message.CorrelationId,
                DateTimeOffset.UtcNow,
                args.Message.Body.ToString());

            await using var scope = _scopeFactory.CreateAsyncScope();
            var db = scope.ServiceProvider.GetRequiredService<NotificationDbContext>();
            var composer = scope.ServiceProvider.GetRequiredService<INotificationComposer>();

            await HandleAsync(db, composer, envelope, args.CancellationToken);
            await args.CompleteMessageAsync(args.Message, args.CancellationToken);
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
                cancellationToken: args.CancellationToken);
        }
    }

    public static async Task<int> HandleAsync(
        NotificationDbContext db,
        INotificationComposer composer,
        PlatformEventEnvelope platformEvent,
        CancellationToken cancellationToken)
    {
        var rules = await db.NotificationRules
            .AsNoTracking()
            .Where(x => x.Enabled && x.EventType == platformEvent.EventType)
            .ToListAsync(cancellationToken);

        if (rules.Count == 0) return 0;

        var existingRuleIds = await db.Notifications
            .Where(x => x.SourceEventId == platformEvent.EventId && x.RuleId != null)
            .Select(x => x.RuleId!.Value)
            .ToListAsync(cancellationToken);

        var existing = existingRuleIds.ToHashSet();

        var notifications = rules
            .Where(rule => !existing.Contains(rule.Id))
            .Select(rule => composer.Compose(rule, platformEvent))
            .ToArray();

        if (notifications.Length == 0) return 0;

        db.Notifications.AddRange(notifications);
        await db.SaveChangesAsync(cancellationToken);

        return notifications.Length;
    }

    private Task ProcessErrorAsync(ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus error. Entity: {Entity}; Source: {Source}.",
            args.EntityPath,
            args.ErrorSource);
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(cancellationToken);
        await base.StopAsync(cancellationToken);
    }

    public override void Dispose()
    {
        _processor.DisposeAsync().AsTask().GetAwaiter().GetResult();
        base.Dispose();
    }
}
