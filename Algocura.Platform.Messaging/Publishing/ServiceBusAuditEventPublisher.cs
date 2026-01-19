using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Events;
using Microsoft.Extensions.Logging;

namespace Algocura.Platform.Messaging.Publishing;

public sealed class ServiceBusAuditEventPublisher(
    IPlatformMessagePublisher publisher,
    ILogger<ServiceBusAuditEventPublisher> logger)
    : IAuditEventPublisher
{
    public async Task PublishAsync(
        EntityAuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(auditEvent);

        var properties =
            new Dictionary<string, object>
            {
                ["serviceName"] = auditEvent.ServiceName,
                ["entityType"] = auditEvent.EntityType,
                ["action"] = auditEvent.Action
            };

        await publisher.PublishAsync(
            auditEvent,
            auditEvent.EventType,
            auditEvent.EventId,
            auditEvent.CorrelationId,
            properties,
            cancellationToken);

        logger.LogInformation(
            "Published audit event {EventType} with ID {EventId} " +
            "for {ServiceName}/{EntityType}/{EntityId}.",
            auditEvent.EventType,
            auditEvent.EventId,
            auditEvent.ServiceName,
            auditEvent.EntityType,
            auditEvent.EntityId);
    }
}
