using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.Platform.Messaging.Publishing;

namespace Algocura.LiteratureIntelligence.Api.Messaging.Publishers;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}

public sealed class ServiceBusEventPublisher(
    IPlatformMessagePublisher publisher,
    ILogger<ServiceBusEventPublisher> logger)
    : IEventPublisher
{
    public async Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent
    {
        ArgumentNullException.ThrowIfNull(
            integrationEvent);

        var applicationProperties =
            new Dictionary<string, object>
            {
                ["eventVersion"] =
                    integrationEvent.EventVersion,

                ["source"] =
                    integrationEvent.Source,

                ["occurredAtUtc"] =
                    integrationEvent.OccurredAtUtc.ToString("O")
            };

        await publisher.PublishAsync(
            integrationEvent,
            integrationEvent.EventType,
            integrationEvent.EventId,
            correlationId,
            applicationProperties,
            cancellationToken);

        logger.LogInformation(
            "Published {EventType} with ID {EventId}.",
            integrationEvent.EventType,
            integrationEvent.EventId);
    }
}