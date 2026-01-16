using Algocura.Platform.Auditing.Events;

namespace Algocura.Platform.Auditing.Abstractions;

/// <summary>
/// Publishes immutable audit events to the configured transport.
/// </summary>
public interface IAuditEventPublisher
{
    Task PublishAsync(
        EntityAuditEvent auditEvent,
        CancellationToken cancellationToken = default);
}

