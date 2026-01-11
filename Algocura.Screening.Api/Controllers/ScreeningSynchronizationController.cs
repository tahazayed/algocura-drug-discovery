using Algocura.Screening.Api.Data;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Algocura.Screening.Api.Messaging.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Controllers;

[ApiController]
[Route("api/v1/screenings/synchronization")]
public sealed class ScreeningSynchronizationController(
    ScreeningDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<ScreeningSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ScreeningSynchronizationResponse>>
        SynchronizeAllCompleted(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;

        var screenings = await dbContext.ScreeningJobs
    .AsNoTracking()
    .Include(screening => screening.Results)
    .Where(
        screening =>
            screening.Status == ScreeningStatus.Completed)
    .ToListAsync(cancellationToken);

        screenings = screenings
            .OrderBy(
                screening =>
                    screening.CompletedAtUtc
                    ?? screening.UpdatedAtUtc)
            .ThenBy(screening => screening.Id)
            .ToList();

        var publishedCount = 0;
        var failures =
            new List<ScreeningSynchronizationFailure>();

        foreach (var screening in screenings)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var integrationEvent =
                    CreateSnapshotEvent(screening);

                await eventPublisher.PublishAsync(
                    integrationEvent,
                    correlationId:
                        $"screening-sync-{screening.Id}",
                    cancellationToken:
                        cancellationToken);

                publishedCount++;
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish synchronization snapshot for " +
                    "screening {ScreeningId}.",
                    screening.Id);

                failures.Add(
                    new ScreeningSynchronizationFailure(
                        screening.Id,
                        screening.Name,
                        exception.Message));
            }
        }

        return Ok(
            new ScreeningSynchronizationResponse(
                screenings.Count,
                publishedCount,
                failures.Count,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                failures));
    }

    [HttpPost("{id:guid}")]
    public async Task<
        ActionResult<ScreeningSynchronizationItemResponse>>
        SynchronizeById(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var screening = await dbContext.ScreeningJobs
            .AsNoTracking()
            .Include(screening => screening.Results)
            .SingleOrDefaultAsync(
                screening => screening.Id == id,
                cancellationToken);

        if (screening is null)
        {
            return NotFound();
        }

        if (screening.Status != ScreeningStatus.Completed)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Screening is not completed",
                    Detail =
                        "Only completed screenings can be synchronized.",
                    Status = StatusCodes.Status409Conflict
                });
        }

        var integrationEvent =
            CreateSnapshotEvent(screening);

        await eventPublisher.PublishAsync(
            integrationEvent,
            correlationId:
                $"screening-sync-{screening.Id}",
            cancellationToken:
                cancellationToken);

        return Ok(
            new ScreeningSynchronizationItemResponse(
                screening.Id,
                screening.Name,
                screening.Results.Count,
                DateTimeOffset.UtcNow));
    }

    private static CompletedScreeningSnapshot
        CreateSnapshotEvent(
            ScreeningJob screening)
    {
        var completedAtUtc =
            screening.CompletedAtUtc
            ?? screening.UpdatedAtUtc;

        var results = screening.Results
            .OrderBy(result => result.Rank)
            .Select(
                result =>
                    new CompletedScreeningSnapshotResult(
                        result.CompoundId,
                        result.Score,
                        result.Rank,
                        result.Selected))
            .ToArray();

        return new CompletedScreeningSnapshot(
            Guid.NewGuid(),
            DateTimeOffset.UtcNow,
            screening.Id,
            screening.Name,
            screening.ScreeningMethod,
            completedAtUtc,
            results);
    }
}

public sealed record ScreeningSynchronizationResponse(
    int TotalCompletedScreenings,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<ScreeningSynchronizationFailure> Failures);

public sealed record ScreeningSynchronizationFailure(
    Guid ScreeningId,
    string ScreeningName,
    string Error);

public sealed record ScreeningSynchronizationItemResponse(
    Guid ScreeningId,
    string ScreeningName,
    int ResultCount,
    DateTimeOffset PublishedAtUtc);