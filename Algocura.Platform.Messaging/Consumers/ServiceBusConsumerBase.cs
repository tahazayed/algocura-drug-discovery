using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Algocura.Platform.Messaging.Consumers;

public abstract class ServiceBusConsumerBase
    : BackgroundService
{
    private readonly ServiceBusProcessor _processor;
    private readonly ILogger _logger;

    protected ServiceBusConsumerBase(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger logger)
    {
        ArgumentNullException.ThrowIfNull(
            client);

        ArgumentNullException.ThrowIfNull(
            options);

        ArgumentNullException.ThrowIfNull(
            logger);

        _logger =
            logger;

        var serviceBusOptions =
            options.Value;

        if (string.IsNullOrWhiteSpace(
                serviceBusOptions.SubscriptionName))
        {
            throw new InvalidOperationException(
                "ServiceBus:SubscriptionName is required for a Service Bus consumer.");
        }

        _processor =
            client.CreateProcessor(
                serviceBusOptions.TopicName,
                serviceBusOptions.SubscriptionName,
                new ServiceBusProcessorOptions
                {
                    AutoCompleteMessages =
                        false,

                    MaxConcurrentCalls =
                        serviceBusOptions.MaxConcurrentCalls,

                    PrefetchCount =
                        serviceBusOptions.PrefetchCount,

                    MaxAutoLockRenewalDuration =
                        TimeSpan.FromMinutes(
                            serviceBusOptions.MaxAutoLockRenewalMinutes)
                });
    }

    protected override async Task ExecuteAsync(
        CancellationToken stoppingToken)
    {
        _processor.ProcessMessageAsync +=
            ProcessMessageInternalAsync;

        _processor.ProcessErrorAsync +=
            ProcessErrorInternalAsync;

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

    protected abstract Task ProcessMessageAsync(
        ProcessMessageEventArgs args);

    protected virtual Task ProcessErrorAsync(
        ProcessErrorEventArgs args)
    {
        _logger.LogError(
            args.Exception,
            "Service Bus consumer error. Entity: {EntityPath}; Source: {ErrorSource}.",
            args.EntityPath,
            args.ErrorSource);

        return Task.CompletedTask;
    }

    protected static string? ResolveEventType(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        return message.ApplicationProperties
            .TryGetValue(
                "eventType",
                out var eventTypeValue)
            ? eventTypeValue?.ToString()
            : message.Subject;
    }

    protected static string ResolveSource(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        return message.ApplicationProperties
            .TryGetValue(
                "source",
                out var sourceValue)
            ? sourceValue?.ToString() ??
              "Unknown"
            : "Unknown";
    }

    protected static Guid ResolveEventId(
        ServiceBusReceivedMessage message)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        return Guid.TryParse(
            message.MessageId,
            out var eventId)
            ? eventId
            : Guid.NewGuid();
    }

    private Task ProcessMessageInternalAsync(
        ProcessMessageEventArgs args) =>
        ProcessMessageAsync(args);

    private Task ProcessErrorInternalAsync(
        ProcessErrorEventArgs args) =>
        ProcessErrorAsync(args);

    public override async Task StopAsync(
        CancellationToken cancellationToken)
    {
        await _processor.StopProcessingAsync(
            cancellationToken);

        await _processor.DisposeAsync();

        await base.StopAsync(
            cancellationToken);
    }
}
