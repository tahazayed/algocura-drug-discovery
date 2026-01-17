using Algocura.Audit.Api.Data;
using Algocura.Audit.Api.Domain;
using Algocura.Audit.Api.Messaging;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Audit.Api.Services;

public interface IAuditIngestionService
{
    Task<bool> IngestAsync(
        IncomingEntityAuditEvent integrationEvent,
        CancellationToken cancellationToken = default);
}

public sealed class AuditIngestionService(
    AuditDbContext dbContext,
    ILogger<AuditIngestionService> logger)
    : IAuditIngestionService
{
    public async Task<bool> IngestAsync(
        IncomingEntityAuditEvent integrationEvent,
        CancellationToken cancellationToken = default)
    {
        if (integrationEvent.EventId == Guid.Empty)
        {
            throw new ArgumentException(
                "Audit event ID is required.",
                nameof(integrationEvent));
        }

        var exists =
            await dbContext.AuditRecords
                .AsNoTracking()
                .AnyAsync(
                    x =>
                        x.EventId ==
                        integrationEvent.EventId,
                    cancellationToken);

        if (exists)
        {
            logger.LogInformation(
                "Audit event {EventId} was already processed.",
                integrationEvent.EventId);

            return false;
        }

        var now =
            DateTimeOffset.UtcNow;

        var record =
            new AuditRecord
            {
                EventId =
                    integrationEvent.EventId,

                OccurredAtUtc =
                    integrationEvent.OccurredAtUtc,

                ServiceName =
                    integrationEvent.ServiceName,

                EntityType =
                    integrationEvent.EntityType,

                EntityId =
                    integrationEvent.EntityId,

                Action =
                    integrationEvent.Action,

                UserId =
                    integrationEvent.UserId,

                Subject =
                    integrationEvent.Subject,

                UserDisplayName =
                    integrationEvent.UserDisplayName,

                UserEmail =
                    integrationEvent.UserEmail,

                ActorType =
                    integrationEvent.ActorType,

                PreviousValuesJson =
                    integrationEvent.PreviousValuesJson,

                CurrentValuesJson =
                    integrationEvent.CurrentValuesJson,

                ChangedPropertiesJson =
                    integrationEvent.ChangedPropertiesJson,

                CorrelationId =
                    integrationEvent.CorrelationId,

                WorkflowId =
                    integrationEvent.WorkflowId,

                TraceId =
                    integrationEvent.TraceId,

                IpAddress =
                    integrationEvent.IpAddress,

                UserAgent =
                    integrationEvent.UserAgent,

                EventType =
                    integrationEvent.EventType,

                EventVersion =
                    integrationEvent.EventVersion,

                ReceivedAtUtc =
                    now,

                CreatedByUserId =
                    SystemUsers.SystemUserId,

                UpdatedByUserId =
                    SystemUsers.SystemUserId,

                CreatedAtUtc =
                    now,

                UpdatedAtUtc =
                    now
            };

        await dbContext.AuditRecords.AddAsync(
            record,
            cancellationToken);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        logger.LogInformation(
            "Stored audit event {EventId} for {ServiceName}/{EntityType}/{EntityId}.",
            record.EventId,
            record.ServiceName,
            record.EntityType,
            record.EntityId);

        return true;
    }
}
