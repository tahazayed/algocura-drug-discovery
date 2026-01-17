using Algocura.Audit.Api.Services;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Audit.Api.Messaging;

public sealed class EntityAuditEventsConsumer(
    ServiceBusClient serviceBusClient,
    IOptions<ServiceBusOptions> options,
    IServiceScopeFactory scopeFactory,
    ILogger<EntityAuditEventsConsumer> logger)
    : BackgroundService
{
    private readonly ServiceBusOptions _options =
        options.Value;

    private ServiceBusProcessor? _processor;

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor =
            serviceBusClient.CreateProcessor(
                _options.TopicName,
                _options.SubscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages = false,
                    MaxConcurrentCalls = 1,
                    PrefetchCount = 0
                });

        _processor.ProcessMessageAsync +=
            ProcessMessageAsync;

        _processor.ProcessErrorAsync +=
            ProcessErrorAsync;

        await _processor.StartProcessingAsync(
            stoppingToken);

        try
        {
            await Task.Delay(
                Timeout.Infinite,
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
            args.Message.Subject;

        if (string.IsNullOrWhiteSpace(eventType) &&
            args.Message.ApplicationProperties.TryGetValue(
                "eventType",
                out var eventTypeValue))
        {
            eventType =
                eventTypeValue?.ToString();
        }

        if (!string.Equals(
                eventType,
                "EntityAuditEvent",
                StringComparison.OrdinalIgnoreCase))
        {
            await args.CompleteMessageAsync(
                args.Message);

            return;
        }

        try
        {
            var integrationEvent =
                JsonSerializer.Deserialize<
                    IncomingEntityAuditEvent>(
                    args.Message.Body.ToString(),
                    new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

            if (integrationEvent is null)
            {
                throw new InvalidOperationException(
                    "EntityAuditEvent payload could not be deserialized.");
            }

            await using var scope =
                scopeFactory.CreateAsyncScope();

            var ingestionService =
                scope.ServiceProvider
                    .GetRequiredService<
                        IAuditIngestionService>();

            await ingestionService.IngestAsync(
                integrationEvent,
                args.CancellationToken);

            await args.CompleteMessageAsync(
                args.Message,
                args.CancellationToken);
        }
        catch (JsonException exception)
        {
            logger.LogError(
                exception,
                "Invalid EntityAuditEvent payload. Dead-lettering message {MessageId}.",
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
        }

        await base.StopAsync(
            cancellationToken);
    }
}
