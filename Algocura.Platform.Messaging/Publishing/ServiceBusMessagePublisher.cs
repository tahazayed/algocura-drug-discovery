using Algocura.Platform.Messaging.Options;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Json;

namespace Algocura.Platform.Messaging.Publishing;

public sealed class ServiceBusMessagePublisher(
    ServiceBusClient client,
    IOptions<ServiceBusOptions> options,
    ILogger<ServiceBusMessagePublisher> logger)
    : IPlatformMessagePublisher,
      IAsyncDisposable
{
    private static readonly JsonSerializerOptions JsonOptions =
        new(JsonSerializerDefaults.Web)
        {
            WriteIndented =
                false
        };

    private readonly ServiceBusOptions _options =
        options.Value;

    private readonly ServiceBusSender _sender =
        client.CreateSender(
            options.Value.TopicName);

    public async Task PublishAsync<TMessage>(
        TMessage message,
        string eventType,
        Guid messageId,
        string? correlationId = null,
        IReadOnlyDictionary<string, object>? applicationProperties = null,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(
            message);

        if (string.IsNullOrWhiteSpace(
                eventType))
        {
            throw new ArgumentException(
                "Event type is required.",
                nameof(eventType));
        }

        if (messageId == Guid.Empty)
        {
            throw new ArgumentException(
                "Message ID is required.",
                nameof(messageId));
        }

        var body =
            BinaryData.FromObjectAsJson(
                message,
                JsonOptions);

        var serviceBusMessage =
            new ServiceBusMessage(body)
            {
                MessageId =
                    messageId.ToString(),

                Subject =
                    eventType,

                CorrelationId =
                    correlationId,

                ContentType =
                    "application/json;charset=utf-8",

                TimeToLive =
                    TimeSpan.FromDays(
                        _options.DefaultMessageTimeToLiveDays)
            };

        serviceBusMessage.ApplicationProperties[
            "eventType"] =
            eventType;

        if (applicationProperties is not null)
        {
            foreach (var property in applicationProperties)
            {
                serviceBusMessage.ApplicationProperties[
                    property.Key] =
                    property.Value;
            }
        }

        await _sender.SendMessageAsync(
            serviceBusMessage,
            cancellationToken);

        logger.LogInformation(
            "Published platform event {EventType} with ID {EventId}.",
            eventType,
            messageId);
    }

    public ValueTask DisposeAsync() =>
        _sender.DisposeAsync();
}
