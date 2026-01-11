using Algocura.CompoundCatalog.Api.Data;
using Algocura.CompoundCatalog.Api.Events;
using Algocura.CompoundCatalog.Api.Messaging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.CompoundCatalog.Api.Controllers;

[ApiController]
[Route("api/v1/compounds/synchronization")]
public sealed class CompoundSynchronizationController(
    CompoundCatalogDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<CompoundSynchronizationController> logger)
    : ControllerBase
{
    /// <summary>
    /// Republishes every compound in the catalog as a CompoundRegistered event.
    /// </summary>
    /// <remarks>
    /// Downstream services should process these messages idempotently and
    /// upsert their local compound projections by CompoundId.
    /// </remarks>
    [HttpPost]
    [ProducesResponseType<CompoundSynchronizationResponse>(
        StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(
        StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<CompoundSynchronizationResponse>>
        SynchronizeAll(
            CancellationToken cancellationToken = default)
    {
        var compounds = await dbContext.Compounds
            .AsNoTracking()
            .OrderBy(compound => compound.Name)
            .ToListAsync(cancellationToken);

        if (compounds.Count == 0)
        {
            return Ok(
                new CompoundSynchronizationResponse(
                    TotalCompounds: 0,
                    PublishedEvents: 0,
                    FailedEvents: 0,
                    StartedAtUtc: DateTimeOffset.UtcNow,
                    CompletedAtUtc: DateTimeOffset.UtcNow,
                    Failures: []));
        }

        var startedAtUtc = DateTimeOffset.UtcNow;
        var publishedEvents = 0;
        var failures =
            new List<CompoundSynchronizationFailure>();

        logger.LogInformation(
            "Starting synchronization of {CompoundCount} compounds.",
            compounds.Count);

        foreach (var compound in compounds)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                var integrationEvent =
                    new CompoundRegistered
                    {
                        CompoundId = compound.Id,
                        Name = compound.Name,
                        CanonicalSmiles =
                            compound.CanonicalSmiles,
                        InchiKey = compound.InchiKey,
                        MolecularWeight =
                            compound.MolecularWeight,
                        MolecularFormula =
                            compound.MolecularFormula,
                        Status = compound.Status
                    };

                await eventPublisher.PublishAsync(
                    integrationEvent,
                    correlationId:
                        $"compound-sync-{compound.Id}",
                    cancellationToken:
                        cancellationToken);

                publishedEvents++;

                logger.LogInformation(
                    "Published synchronization event for compound " +
                    "{CompoundId} ({CompoundName}).",
                    compound.Id,
                    compound.Name);
            }
            catch (OperationCanceledException)
                when (cancellationToken.IsCancellationRequested)
            {
                logger.LogWarning(
                    "Compound synchronization was cancelled.");

                throw;
            }
            catch (Exception exception)
            {
                logger.LogError(
                    exception,
                    "Failed to publish synchronization event for " +
                    "compound {CompoundId} ({CompoundName}).",
                    compound.Id,
                    compound.Name);

                failures.Add(
                    new CompoundSynchronizationFailure(
                        CompoundId: compound.Id,
                        CompoundName: compound.Name,
                        Error: exception.Message));
            }
        }

        var completedAtUtc = DateTimeOffset.UtcNow;

        var response =
            new CompoundSynchronizationResponse(
                TotalCompounds: compounds.Count,
                PublishedEvents: publishedEvents,
                FailedEvents: failures.Count,
                StartedAtUtc: startedAtUtc,
                CompletedAtUtc: completedAtUtc,
                Failures: failures);

        logger.LogInformation(
            "Compound synchronization completed. " +
            "Published: {PublishedCount}; Failed: {FailedCount}.",
            publishedEvents,
            failures.Count);

        return Ok(response);
    }
}

public sealed record CompoundSynchronizationResponse(
    int TotalCompounds,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<CompoundSynchronizationFailure> Failures);

public sealed record CompoundSynchronizationFailure(
    Guid CompoundId,
    string CompoundName,
    string Error);
