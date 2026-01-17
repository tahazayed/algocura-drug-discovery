using Algocura.Audit.Api.Messaging;
using Algocura.Audit.Api.Services;
using Algocura.Audit.Api.Tests.Support;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;

namespace Algocura.Audit.Api.Tests.Services;

public sealed class AuditIngestionServiceTests
{
    [Fact]
    public async Task IngestAsync_NewEvent_StoresAuditRecord()
    {
        await using var dbContext =
    await TestDbContextFactory.CreateAsync();

        await using (dbContext)
        {
            var service =
                new AuditIngestionService(
                    dbContext,
                    NullLogger<
                        AuditIngestionService>.Instance);

            var eventId =
                Guid.NewGuid();

            var auditEvent =
                new IncomingEntityAuditEvent
                {
                    EventId =
                        eventId,

                    OccurredAtUtc =
                        DateTimeOffset.UtcNow,

                    ServiceName =
                        "Algocura.Screening.Api",

                    EntityType =
                        "ScreeningJob",

                    EntityId =
                        Guid.NewGuid().ToString(),

                    Action =
                        "Completed",

                    ActorType =
                        "System",

                    EventType =
                        "EntityAuditEvent",

                    EventVersion =
                        "1.0"
                };

            var inserted =
                await service.IngestAsync(
                    auditEvent);

            inserted.Should().BeTrue();

            var record =
                await dbContext.AuditRecords
                    .FindAsync(eventId);

            record.Should().NotBeNull();
            record!.ServiceName.Should()
                .Be("Algocura.Screening.Api");
            record.Action.Should()
                .Be("Completed");
        }
    }

    [Fact]
    public async Task IngestAsync_DuplicateEvent_IsIdempotent()
    {
        await using var dbContext =
    await TestDbContextFactory.CreateAsync();

        await using (dbContext)
        {
            var service =
                new AuditIngestionService(
                    dbContext,
                    NullLogger<
                        AuditIngestionService>.Instance);

            var auditEvent =
                new IncomingEntityAuditEvent
                {
                    EventId =
                        Guid.NewGuid(),

                    OccurredAtUtc =
                        DateTimeOffset.UtcNow,

                    ServiceName =
                        "Algocura.TargetDiscovery.Api",

                    EntityType =
                        "TargetAssessment",

                    EntityId =
                        Guid.NewGuid().ToString(),

                    Action =
                        "Updated",

                    ActorType =
                        "System",

                    EventType =
                        "EntityAuditEvent",

                    EventVersion =
                        "1.0"
                };

            var first =
                await service.IngestAsync(
                    auditEvent);

            var second =
                await service.IngestAsync(
                    auditEvent);

            first.Should().BeTrue();
            second.Should().BeFalse();

            dbContext.AuditRecords.Should()
                .ContainSingle();
        }
    }
}
