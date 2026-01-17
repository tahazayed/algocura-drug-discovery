using Algocura.Audit.Api.Controllers;
using Algocura.Audit.Api.Domain;
using Algocura.Audit.Api.Tests.Support;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;

namespace Algocura.Audit.Api.Tests.Controllers;

public sealed class AuditControllerTests
{
    [Fact]
    public async Task Search_ServiceNameFilter_ReturnsMatchingRecord()
    {
        await using var dbContext =
    await TestDbContextFactory.CreateAsync();

        await using (dbContext)
        {
            var now =
                DateTimeOffset.UtcNow;

            dbContext.AuditRecords.Add(
                new AuditRecord
                {
                    EventId =
                        Guid.NewGuid(),

                    OccurredAtUtc =
                        now,

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
                        "1.0",

                    ReceivedAtUtc =
                        now,

                    CreatedAtUtc =
                        now,

                    UpdatedAtUtc =
                        now
                });

            await dbContext.SaveChangesAsync();

            var controller =
                new AuditController(
                    dbContext);

            var result =
                await controller.Search(
                    "Algocura.Screening.Api",
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    null,
                    1,
                    50);

            var ok =
                result.Result.Should()
                    .BeOfType<OkObjectResult>()
                    .Subject;

            ok.Value.Should().NotBeNull();
        }
    }

    [Fact]
    public async Task Summary_ReturnsTotals()
    {
        await using var dbContext =
    await TestDbContextFactory.CreateAsync();

        await using (dbContext)
        {
            var now =
                DateTimeOffset.UtcNow;

            dbContext.AuditRecords.Add(
                new AuditRecord
                {
                    EventId =
                        Guid.NewGuid(),

                    OccurredAtUtc =
                        now,

                    ServiceName =
                        "Algocura.Workflow.Api",

                    EntityType =
                        "DiscoveryWorkflow",

                    EntityId =
                        Guid.NewGuid().ToString(),

                    Action =
                        "Completed",

                    ActorType =
                        "System",

                    EventType =
                        "EntityAuditEvent",

                    EventVersion =
                        "1.0",

                    ReceivedAtUtc =
                        now,

                    CreatedAtUtc =
                        now,

                    UpdatedAtUtc =
                        now
                });

            await dbContext.SaveChangesAsync();

            var controller =
                new AuditController(
                    dbContext);

            var result =
                await controller.Summary();

            result.Result.Should()
                .BeOfType<OkObjectResult>();
        }
    }
}
