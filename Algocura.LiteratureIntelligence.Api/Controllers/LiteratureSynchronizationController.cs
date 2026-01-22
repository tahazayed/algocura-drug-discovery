using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Controllers;

[Authorize(Roles = "Service,Administrator")]
[ApiController]
[Route("api/v1/literature-intelligence/synchronization")]
public sealed class LiteratureSynchronizationController(
    LiteratureIntelligenceDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<LiteratureSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<LiteratureSynchronizationResponse>>
        SynchronizePublishedHypotheses(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;

        var hypotheses = await dbContext.TargetHypotheses
            .AsNoTracking()
            .Include(x => x.EvidenceLinks)
            .Where(x => x.Status == TargetHypothesisStatus.Published)
            .ToListAsync(cancellationToken);

        hypotheses = hypotheses
            .OrderBy(x => x.DiseaseName)
            .ThenBy(x => x.GeneSymbol)
            .ThenBy(x => x.Id)
            .ToList();

        var publishedCount = 0;
        var failures =
            new List<LiteratureSynchronizationFailure>();

        foreach (var hypothesis in hypotheses)
        {
            try
            {
                await eventPublisher.PublishAsync(
                    new LiteratureIntelligenceSnapshot(
                        Guid.NewGuid(),
                        DateTimeOffset.UtcNow,
                        hypothesis.Id,
                        hypothesis.DiseaseName,
                        hypothesis.GeneSymbol,
                        hypothesis.ProteinTarget,
                        hypothesis.DesiredModulation,
                        hypothesis.Rationale,
                        hypothesis.EvidenceScore,
                        hypothesis.EvidenceLinks
                            .Select(x => x.EvidenceStatementId)
                            .ToArray()),
                    correlationId:
                        $"literature-sync-{hypothesis.Id}",
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
                    "Failed to synchronize target hypothesis {TargetHypothesisId}.",
                    hypothesis.Id);

                failures.Add(
                    new LiteratureSynchronizationFailure(
                        hypothesis.Id,
                        hypothesis.DiseaseName,
                        hypothesis.GeneSymbol,
                        exception.Message));
            }
        }

        return Ok(
            new LiteratureSynchronizationResponse(
                hypotheses.Count,
                publishedCount,
                failures.Count,
                startedAtUtc,
                DateTimeOffset.UtcNow,
                failures));
    }
}

public sealed record LiteratureSynchronizationResponse(
    int TotalPublishedHypotheses,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<LiteratureSynchronizationFailure> Failures);

public sealed record LiteratureSynchronizationFailure(
    Guid TargetHypothesisId,
    string DiseaseName,
    string GeneSymbol,
    string Error);
