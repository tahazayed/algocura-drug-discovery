using Algocura.Reporting.Api.Data;
using Algocura.Reporting.Api.Domain;
using Algocura.Reporting.Api.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Controllers;

[ApiController]
[Route("api/v1/reporting/synchronization")]
public sealed class ReportingSynchronizationController(
    ReportingDbContext db,
    IEventPublisher publisher)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ReportingSynchronizationResponse>> Synchronize(
        CancellationToken cancellationToken = default)
    {
        var projections = await db.ReportingProjections.CountAsync(
            cancellationToken);

        var definitions = await db.ReportDefinitions.CountAsync(
            x => x.Status == ReportDefinitionStatus.Active,
            cancellationToken);

        var runs = await db.ReportRuns.CountAsync(
            x => x.Status == ReportRunStatus.Completed,
            cancellationToken);

        var snapshot = new ReportingSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            projections,
            definitions,
            runs);

        await publisher.PublishAsync(
            snapshot,
            cancellationToken: cancellationToken);

        return Ok(new ReportingSynchronizationResponse(
            projections,
            definitions,
            runs,
            snapshot.EventId,
            snapshot.OccurredAtUtc));
    }
}

public sealed record ReportingSynchronizationResponse(
    int ProjectionCount,
    int ActiveReportDefinitions,
    int CompletedReportRuns,
    Guid EventId,
    DateTimeOffset PublishedAtUtc);
