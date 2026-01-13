using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Messaging.Publishers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Controllers;

[ApiController]
[Route("api/v1/chemical-design/synchronization")]
public sealed class ChemicalDesignSynchronizationController(
    ChemicalDesignDbContext db,
    IEventPublisher publisher,
    ILogger<ChemicalDesignSynchronizationController> logger)
    : ControllerBase
{
    [HttpPost]
    public async Task<ActionResult<ChemicalDesignSynchronizationResponse>>
        SynchronizeSelectedCandidates(
            CancellationToken cancellationToken = default)
    {
        var startedAtUtc = DateTimeOffset.UtcNow;

        var candidates = await db.CandidateStructures
            .AsNoTracking()
            .Include(x => x.DesignProject)
            .ThenInclude(x => x!.SelectedTarget)
            .Where(x => x.Status == CandidateStructureStatus.SelectedForRegistration)
            .ToListAsync(cancellationToken);

        candidates = candidates
            .OrderBy(x => x.DesignProject!.Name)
            .ThenBy(x => x.Name)
            .ThenBy(x => x.Id)
            .ToList();

        var published = 0;
        var failures = new List<ChemicalDesignSynchronizationFailure>();

        foreach (var candidate in candidates)
        {
            try
            {
                await publisher.PublishAsync(
                    CreateSnapshot(candidate),
                    $"chemical-design-sync-{candidate.Id}",
                    cancellationToken);
                published++;
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
                    "Failed to synchronize candidate {CandidateStructureId}.",
                    candidate.Id);

                failures.Add(new ChemicalDesignSynchronizationFailure(
                    candidate.Id,
                    candidate.DesignProjectId,
                    exception.Message));
            }
        }

        return Ok(new ChemicalDesignSynchronizationResponse(
            candidates.Count,
            published,
            failures.Count,
            startedAtUtc,
            DateTimeOffset.UtcNow,
            failures));
    }

    [HttpPost("{id:guid}")]
    public async Task<ActionResult<ChemicalDesignSynchronizationItemResponse>>
        SynchronizeById(
            Guid id,
            CancellationToken cancellationToken = default)
    {
        var candidate = await db.CandidateStructures
            .AsNoTracking()
            .Include(x => x.DesignProject)
            .ThenInclude(x => x!.SelectedTarget)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (candidate is null) return NotFound();

        if (candidate.Status != CandidateStructureStatus.SelectedForRegistration)
            return Conflict(new ProblemDetails
            {
                Title = "Candidate is not selected for registration",
                Detail = "Only selected candidates can be synchronized.",
                Status = StatusCodes.Status409Conflict
            });

        var publishedAtUtc = DateTimeOffset.UtcNow;

        await publisher.PublishAsync(
            CreateSnapshot(candidate, publishedAtUtc),
            $"chemical-design-sync-{candidate.Id}",
            cancellationToken);

        return Ok(new ChemicalDesignSynchronizationItemResponse(
            candidate.Id,
            candidate.DesignProjectId,
            candidate.Name,
            publishedAtUtc));
    }

    private static ChemicalDesignSnapshot CreateSnapshot(
        CandidateStructure candidate,
        DateTimeOffset? occurredAtUtc = null)
    {
        var project = candidate.DesignProject
            ?? throw new InvalidOperationException("Design project was not loaded.");
        var target = project.SelectedTarget
            ?? throw new InvalidOperationException("Selected target was not loaded.");

        return new ChemicalDesignSnapshot(
            Guid.NewGuid(),
            occurredAtUtc ?? DateTimeOffset.UtcNow,
            candidate.Id,
            project.Id,
            target.TargetAssessmentId,
            target.TargetHypothesisId,
            target.DiseaseName,
            target.GeneSymbol,
            target.ProteinTarget,
            target.DesiredModulation,
            candidate.Name,
            candidate.CanonicalSmiles,
            candidate.InchiKey,
            candidate.MolecularFormula,
            candidate.MolecularWeight,
            candidate.MatchScore,
            candidate.SelectionRationale ?? "Selected for compound registration.");
    }
}

public sealed record ChemicalDesignSynchronizationResponse(
    int TotalSelectedCandidates,
    int PublishedEvents,
    int FailedEvents,
    DateTimeOffset StartedAtUtc,
    DateTimeOffset CompletedAtUtc,
    IReadOnlyCollection<ChemicalDesignSynchronizationFailure> Failures);

public sealed record ChemicalDesignSynchronizationFailure(
    Guid CandidateStructureId,
    Guid DesignProjectId,
    string Error);

public sealed record ChemicalDesignSynchronizationItemResponse(
    Guid CandidateStructureId,
    Guid DesignProjectId,
    string CandidateName,
    DateTimeOffset PublishedAtUtc);
