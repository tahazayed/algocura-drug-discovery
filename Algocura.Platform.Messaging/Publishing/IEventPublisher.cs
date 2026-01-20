using Algocura.Platform.Messaging.Events;

namespace Algocura.Platform.Messaging.Publishing;

public interface IEventPublisher
{
    Task PublishAsync<TEvent>(
        TEvent integrationEvent,
        string? correlationId = null,
        CancellationToken cancellationToken = default)
        where TEvent : IIntegrationEvent;
}
