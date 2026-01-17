using Algocura.Reporting.Api.Data;
using Algocura.Reporting.Api.Domain;
using Algocura.Reporting.Api.Services;
using FluentAssertions;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Tests;

public sealed class ReportExecutionServiceTests
{
    [Fact]
    public async Task ExecuteAsync_PlatformSummary_CreatesCompletedRun()
    {
        await using var connection =
            new SqliteConnection("Data Source=:memory:");

        await connection.OpenAsync();

        var options =
            new DbContextOptionsBuilder<ReportingDbContext>()
                .UseSqlite(connection)
                .Options;

        await using var db =
            new ReportingDbContext(options);

        await db.Database.EnsureCreatedAsync();

        var definition = new ReportDefinition
        {
            Id = Guid.NewGuid(),
            Name = "Platform Summary",
            ReportType = "PlatformSummary",
            Status = ReportDefinitionStatus.Active,
            CreatedAtUtc = DateTimeOffset.UtcNow,
            UpdatedAtUtc = DateTimeOffset.UtcNow
        };

        db.ReportDefinitions.Add(definition);

        db.ReportingProjections.Add(
            new ReportingProjection
            {
                Id = Guid.NewGuid(),
                SourceEntityId = Guid.NewGuid(),
                ProjectionType =
                    ReportingProjectionType.Workflow,
                SourceEventType =
                    "WorkflowSnapshot",
                SourceService =
                    "Algocura.Workflow.Api",
                Title =
                    "Workflow",
                Outcome =
                    "Completed",
                SourceUpdatedAtUtc =
                    DateTimeOffset.UtcNow,
                SynchronizedAtUtc =
                    DateTimeOffset.UtcNow,
                CreatedAtUtc =
                    DateTimeOffset.UtcNow,
                UpdatedAtUtc =
                    DateTimeOffset.UtcNow
            });

        await db.SaveChangesAsync();

        var service =
            new ReportExecutionService(db);

        var run = await service.ExecuteAsync(
            definition,
            "Test Run",
            "{}");

        run.Status.Should()
            .Be(ReportRunStatus.Completed);

        run.Metrics.Should()
            .ContainSingle(
                x =>
                    x.Name ==
                    "CompletedWorkflowCount" &&
                    x.Value == 1m);
    }
}
