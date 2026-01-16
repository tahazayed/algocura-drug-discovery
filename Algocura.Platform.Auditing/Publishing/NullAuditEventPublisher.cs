using Algocura.Platform.Auditing.Abstractions;
using Algocura.Platform.Auditing.Events;

namespace Algocura.Platform.Auditing.Publishing;

/// <summary>
/// Default publisher used when a service only needs entity stamping.
/// Replace this registration with a Service Bus publisher to emit audit events.
/// </summary>
public sealed class NullAuditEventPublisher
    : IAuditEventPublisher
{
    public Task PublishAsync(
        EntityAuditEvent auditEvent,
        CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

