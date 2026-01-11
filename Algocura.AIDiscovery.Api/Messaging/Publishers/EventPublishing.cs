using Algocura.AIDiscovery.Api.Events;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.AIDiscovery.Api.Messaging.Publishers;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}

public sealed class ServiceBusEventPublisher
    : IEventPublisher,
      IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web);

    private readonly ServiceBusSender _sender;
    private readonly ILogger<ServiceBusEventPublisher> _logger;

    public ServiceBusEventPublisher(
        ServiceBusClient client,
        IOptions<ServiceBusOptions> options,
        ILogger<ServiceBusEventPublisher> logger)
    {
        _logger = logger;
        _sender = client.CreateSender(options.Value.TopicName);
    }

    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        var message = new ServiceBusMessage(
            BinaryData.FromObjectAsJson(
                integrationEvent,
                JsonOptions))
        {
            MessageId = integrationEvent.EventId.ToString(),
            Subject = integrationEvent.EventType,
            ContentType = "application/json;charset=utf-8",
            CorrelationId = correlationId,
            TimeToLive = TimeSpan.FromDays(7)
        };

        message.ApplicationProperties["eventType"] =
            integrationEvent.EventType;
        message.ApplicationProperties["eventVersion"] =
            integrationEvent.EventVersion;
        message.ApplicationProperties["source"] =
            integrationEvent.Source;
        message.ApplicationProperties["occurredAtUtc"] =
            integrationEvent.OccurredAtUtc.ToString("O");

        await _sender.SendMessageAsync(message, cancellationToken);

        _logger.LogInformation(
            "Published {EventType} with ID {EventId}.",
            integrationEvent.EventType,
            integrationEvent.EventId);
    }

    public ValueTask DisposeAsync() => _sender.DisposeAsync();
}
