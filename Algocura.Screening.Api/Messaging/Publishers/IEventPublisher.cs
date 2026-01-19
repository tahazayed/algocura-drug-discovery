using Algocura.Screening.Api.Events;

namespace Algocura.Screening.Api.Messaging.Publishers;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}