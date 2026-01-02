using Algocura.CompoundCatalog.Api.Events;

namespace Algocura.CompoundCatalog.Api.Messaging;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IntegrationEvent;
}