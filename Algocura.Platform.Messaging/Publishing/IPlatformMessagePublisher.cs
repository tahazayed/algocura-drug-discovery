namespace Algocura.Platform.Messaging.Publishing;

public interface IPlatformMessagePublisher
{
    Task PublishAsync<TMessage>(
        TMessage message,
        string eventType,
        Guid messageId,
        string? correlationId = null,
        IReadOnlyDictionary<string, object>? applicationProperties = null,
        CancellationToken cancellationToken = default);
}
