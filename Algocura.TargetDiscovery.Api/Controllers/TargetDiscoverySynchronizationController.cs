using Algocura.TargetDiscovery.Api.Data;
using Algocura.TargetDiscovery.Api.Domain;
using Algocura.TargetDiscovery.Api.Events;
using Algocura.TargetDiscovery.Api.Messaging.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Controllers;

[ApiController]
[Route("api/v1/target-discovery/synchronization")]
public sealed class TargetDiscoverySynchronizationController(
    TargetDiscoveryDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<TargetDiscoverySynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<TargetDiscoverySynchronizationResponse>>
        SynchronizeSelectedTargets(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc =
            DateTimeOffset.UtcNow;

        var assessments = await dbContext.TargetAssessments
            .AsNoTracking()
            .Include(x => x.TargetHypothesis)
            .Where(
                x =>
                    x.Status ==
                    TargetAssessmentStatus.Selected)
            .ToListAsync(cancellationToken);

        assessments = assessments
            .OrderBy(
                x =>
                    x.TargetHypothesis!.DiseaseName)
            .ThenBy(
                x =>
                    x.TargetHypothesis!.GeneSymbol)
            .ThenBy(x => x.Id)
            .ToList();

        var publishedCount = 0;
        var failures =
            new List<TargetDiscoverySynchronizationFailure>();

        foreach (var assessment in assessments)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var hypothesis = assessment.TargetHypothesis
                    ?? throw new InvalidOperationException(
                        "The target hypothesis could not be loaded.");

                await eventPublisher.PublishAsync(
                    new TargetDiscoverySnapshot(
                        Guid.NewGuid(),
                        DateTimeOffset.UtcNow,
                        assessment.Id,
                        hypothesis.TargetHypothesisId,
                        hypothesis.DiseaseName,
                        hypothesis.GeneSymbol,
                        hypothesis.ProteinTarget,
                        hypothesis.DesiredModulation,
                        hypothesis.Rationale,
                        assessment.LiteratureEvidenceScore,
                        assessment.OverallScore,
                        assessment.ConfidenceLevel.ToString()),
                    correlationId:
                        $"target-sync-{assessment.Id}",
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
                    "Failed to synchronize target assessment {TargetAssessmentId}.",
                    assessment.Id);

                failures.Add(
                    new TargetDiscoverySynchronizationFailure(
                        assessment.Id,
                        assessment.TargetHypothesisId,
                        exception.Message));
            }
        }

        return Ok(
            new TargetDiscoverySynchronizationResponse(
                assessments.Count,
                publishedCount,
                failures.Count,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                failures));
    }

    [HttpPost("{id:guid}")]
    public async Task<
        ActionResult<TargetDiscoverySynchronizationItemResponse>>
        SynchronizeById(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var assessment = await dbContext.TargetAssessments
            .AsNoTracking()
            .Include(x => x.TargetHypothesis)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assessment is null)
        {
            return NotFound();
        }

        if (assessment.Status !=
            TargetAssessmentStatus.Selected)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Target is not selected",
                    Detail =
                        "Only selected target assessments can be synchronized.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var hypothesis = assessment.TargetHypothesis
            ?? throw new InvalidOperationException(
                "The target hypothesis could not be loaded.");

        var publishedAtUtc =
            DateTimeOffset.UtcNow;

        await eventPublisher.PublishAsync(
            new TargetDiscoverySnapshot(
                Guid.NewGuid(),
                publishedAtUtc,
                assessment.Id,
                hypothesis.TargetHypothesisId,
                hypothesis.DiseaseName,
                hypothesis.GeneSymbol,
                hypothesis.ProteinTarget,
                hypothesis.DesiredModulation,
                hypothesis.Rationale,
                assessment.LiteratureEvidenceScore,
                assessment.OverallScore,
                assessment.ConfidenceLevel.ToString()),
            correlationId:
                $"target-sync-{assessment.Id}",
            cancellationToken:
                cancellationToken);

        return Ok(
            new TargetDiscoverySynchronizationItemResponse(
                assessment.Id,
                hypothesis.TargetHypothesisId,
                hypothesis.GeneSymbol,
                publishedAtUtc));
    }
}

public sealed record TargetDiscoverySynchronizationResponse(
    int TotalSelectedTargets,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<TargetDiscoverySynchronizationFailure> Failures);

public sealed record TargetDiscoverySynchronizationFailure(
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    string Error);

public sealed record TargetDiscoverySynchronizationItemResponse(
    Guid TargetAssessmentId,
    Guid TargetHypothesisId,
    string GeneSymbol,
    DateTimeOffset PublishedAtUtc);
