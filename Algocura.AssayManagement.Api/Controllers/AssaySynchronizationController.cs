using Algocura.AssayManagement.Api.Data;
using Algocura.AssayManagement.Api.Domain;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Controllers;

[Authorize(Roles = "Service,Administrator")]
[ApiController]
[Route("api/v1/assay-management/synchronization")]
public sealed class AssaySynchronizationController(
    AssayManagementDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<AssaySynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<AssaySynchronizationResponse>>
        SynchronizePublishedAssays(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc =
            DateTimeOffset.UtcNow;

        var assays = await dbContext.Assays
            .AsNoTracking()
            .Include(x => x.Results)
            .Where(
                x => x.Status == AssayStatus.Published)
            .OrderBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToListAsync(cancellationToken);

        var publishedCount = 0;
        var failures =
            new List<AssaySynchronizationFailure>();

        foreach (var assay in assays)
        {
            try
            {
                await eventPublisher.PublishAsync(
                    AssaysController.CreateSnapshot(
                        assay,
                        DateTimeOffset.UtcNow),
                    $"assay-sync-{assay.Id}",
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
                    "Failed to synchronize assay {AssayId}.",
                    assay.Id);

                failures.Add(
                    new AssaySynchronizationFailure(
                        assay.Id,
                        assay.ScreeningId,
                        exception.Message));
            }
        }

        return Ok(
            new AssaySynchronizationResponse(
                assays.Count,
                publishedCount,
                failures.Count,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                failures));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<AssaySynchronizationItemResponse>>
        SynchronizeById(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var assay = await dbContext.Assays
            .AsNoTracking()
            .Include(x => x.Results)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assay is null)
        {
            return NotFound();
        }

        if (assay.Status !=
            AssayStatus.Published)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assay is not published",
                    Detail =
                        "Only published assays can be synchronized.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var publishedAtUtc =
            DateTimeOffset.UtcNow;

        await eventPublisher.PublishAsync(
            AssaysController.CreateSnapshot(
                assay,
                publishedAtUtc),
            $"assay-sync-{assay.Id}",
            cancellationToken);

        return Ok(
            new AssaySynchronizationItemResponse(
                assay.Id,
                assay.ScreeningId,
                assay.Name,
                publishedAtUtc));
    }
}

public sealed record AssaySynchronizationResponse(
    int TotalPublishedAssays,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<AssaySynchronizationFailure> Failures);

public sealed record AssaySynchronizationFailure(
    Guid AssayId,
    Guid ScreeningId,
    string Error);

public sealed record AssaySynchronizationItemResponse(
    Guid AssayId,
    Guid ScreeningId,
    string AssayName,
    DateTimeOffset PublishedAtUtc);

