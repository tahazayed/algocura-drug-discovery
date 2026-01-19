using Algocura.Audit.Api.Services;
using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Audit.Api.Messaging;

public sealed class EntityAuditEventsConsumer(
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusOptions> serviceBusOptions,
    IConfiguration configuration,
    IServiceScopeFactory scopeFactory,
    ILogger<EntityAuditEventsConsumer> logger)
    : BackgroundService
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            PropertyNameCaseInsensitive =
                true
        };

    private readonly string _topicName =
        serviceBusOptions.Value.TopicName;

    private readonly string _subscriptionName =
        configuration["ServiceBus:SubscriptionName"]
        ?? throw new InvalidOperationException(
            "ServiceBus:SubscriptionName is required.");

    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor =
            serviceBusClient.CreateProcessor(
                _topicName,
                _subscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages =
                        false,

                    MaxConcurrentCalls =
                        1,

                    PrefetchCount =
                        0,

                    MaxAutoLockRenewalDuration =
                        TimeSpan.FromMinutes(5)
                });

        _processor.ProcessMessageAsync +=
            ProcessMessageAsync;

        _processor.ProcessErrorAsync +=
            ProcessErrorAsync;

        await _processor.StartProcessingAsync(
            stoppingToken);

        logger.LogInformation(
            "Audit consumer started. Topic={TopicName}, " +
            "Subscription={SubscriptionName}.",
            _topicName,
            _subscriptionName);

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
        var eventType =
            args.Message.ApplicationProperties
                .TryGetValue(
                    "eventType",
                    out var eventTypeValue)
                ? eventTypeValue?.ToString()
                : args.Message.Subject;

        if (!string.Equals(
                eventType,
                "EntityAuditEvent",
                StringComparison.OrdinalIgnoreCase))
        {
            logger.LogDebug(
                "Ignoring unsupported event type {EventType}.",
                eventType);

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);

            return;
        }

        try
        {
            var integrationEvent =
                args.Message.Body
                    .ToObjectFromJson<
                        IncomingEntityAuditEvent>(
                        JsonOptions);

            if (integrationEvent is null)
            {
                throw new JsonException(
                    "EntityAuditEvent payload is empty.");
            }

            await using var scope =
                scopeFactory.CreateAsyncScope();

            var ingestionService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IAuditIngestionService>();

            var inserted =
                await ingestionService.IngestAsync(
                    integrationEvent,
                    args.CancellationToken);

            if (inserted)
            {
                logger.LogInformation(
                    "Processed audit event {EventId} from " +
                    "{ServiceName} for {EntityType} {EntityId}.",
                    integrationEvent.EventId,
                    integrationEvent.ServiceName,
                    integrationEvent.EntityType,
                    integrationEvent.EntityId);
            }
            else
            {
                logger.LogInformation(
                    "Audit event {EventId} was already processed.",
                    integrationEvent.EventId);
            }

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Invalid EntityAuditEvent payload. " +
                "Dead-lettering message {MessageId}.",
                args.Message.MessageId);

            await args.DeadLetterMessageAsync(
                args.Message,
                "InvalidAuditPayload",
                exception.Message,
                args.CancellationToken);
        }
        catch (Exception exception)
        {
            logger.LogError(
                exception,
                "Failed to process audit message {MessageId}.",
                args.Message.MessageId);

            await args.AbandonMessageAsync(
                args.Message,
                cancellationToken:
                    args.CancellationToken);
        }
    }

    private Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        logger.LogError(
            args.Exception,
            "Audit Service Bus processor error. " +
            "Entity={EntityPath}, Source={ErrorSource}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        if (_processor is not null)
        {
            await _processor.StopProcessingAsync(
                cancellationToken);

            await _processor.DisposeAsync();

            _processor =
                null;
        }

        await base.StopAsync(
            cancellationToken);
    }
}