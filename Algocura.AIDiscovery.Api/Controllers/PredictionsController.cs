using Algocura.AIDiscovery.Api.Contracts;
using Algocura.AIDiscovery.Api.Data;
using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Events;
using Algocura.AIDiscovery.Api.Mapping;
using Algocura.AIDiscovery.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AIDiscovery.Api.Controllers;

[ApiController]
[Route("api/v1/predictions")]
public sealed class PredictionsController(
    AIDiscoveryDbContext db,
    IPredictionScoringEngine scoringEngine,
    IEventPublisher publisher,
    ILogger<PredictionsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<PredictionResponse>>> GetAll(
            PredictionStatus? status,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        take = Math.Clamp(take, 1, 200);
        skip = Math.Max(skip, 0);

        var query = db.PredictionJobs
            .AsNoTracking()
            .Include(x => x.Candidates)
            .Include(x => x.Results)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var jobs = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(jobs.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PredictionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await LoadJobAsync(id, cancellationToken);

        return job is null
            ? NotFound()
            : Ok(job.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<PredictionResponse>> Create(
        CreatePredictionRequest request,
        CancellationToken cancellationToken)
    {
        var model = await db.ModelDefinitions
            .SingleOrDefaultAsync(
                x =>
                    x.Id == request.ModelDefinitionId &&
                    x.Status == ModelStatus.Active,
                cancellationToken);

        if (model is null)
        {
            ModelState.AddModelError(
                nameof(request.ModelDefinitionId),
                "The requested active model was not found.");

            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var candidates = new List<PredictionCandidate>();

        if (request.SourceScreeningId.HasValue)
        {
            var screening = await db.Screenings
                .AsNoTracking()
                .Include(x => x.Results)
                .SingleOrDefaultAsync(
                    x => x.ScreeningId == request.SourceScreeningId.Value,
                    cancellationToken);

            if (screening is null)
            {
                ModelState.AddModelError(
                    nameof(request.SourceScreeningId),
                    "The source screening projection was not found.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var ids = screening.Results
                .Select(x => x.CompoundId)
                .ToArray();

            var compounds = await db.Compounds
                .AsNoTracking()
                .Where(x => ids.Contains(x.CompoundId))
                .ToDictionaryAsync(x => x.CompoundId, cancellationToken);

            foreach (var result in screening.Results)
            {
                if (compounds.TryGetValue(
                    result.CompoundId,
                    out var compound))
                {
                    candidates.Add(
                        CreateCandidate(
                            compound,
                            result.Score,
                            result.Rank,
                            result.Selected));
                }
            }
        }
        else
        {
            var ids = request.CompoundIds?
                .Distinct()
                .ToArray() ?? [];

            if (ids.Length == 0)
            {
                ModelState.AddModelError(
                    nameof(request.CompoundIds),
                    "At least one compound ID is required.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            var compounds = await db.Compounds
                .AsNoTracking()
                .Where(x => ids.Contains(x.CompoundId))
                .ToListAsync(cancellationToken);

            var found = compounds
                .Select(x => x.CompoundId)
                .ToHashSet();

            var missing = ids
                .Where(x => !found.Contains(x))
                .ToArray();

            if (missing.Length > 0)
            {
                ModelState.AddModelError(
                    nameof(request.CompoundIds),
                    $"Unknown compound IDs: {string.Join(", ", missing)}");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }

            candidates.AddRange(
                compounds.Select(
                    compound =>
                        CreateCandidate(
                            compound,
                            0m,
                            0,
                            false)));
        }

        var now = DateTimeOffset.UtcNow;

        var job = new PredictionJob
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            ModelDefinitionId = model.Id,
            SourceScreeningId = request.SourceScreeningId,
            Description = request.Description?.Trim(),
            Status = PredictionStatus.Queued,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var candidate in candidates)
        {
            candidate.PredictionJobId = job.Id;
            job.Candidates.Add(candidate);
        }

        db.PredictionJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new PredictionCreated(
                Guid.NewGuid(),
                now,
                job.Id,
                job.Name,
                model.Id,
                job.Candidates.Count),
            correlationId: job.Id.ToString(),
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Created prediction {PredictionId}.",
            job.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = job.Id },
            job.ToResponse());
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<PredictionResponse>> Start(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await LoadJobAsync(id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (job.Status != PredictionStatus.Queued)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid prediction status",
                Detail = "Only queued predictions can be started.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var now = DateTimeOffset.UtcNow;
        job.Status = PredictionStatus.Running;
        job.StartedAtUtc = now;
        job.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new PredictionStarted(
                Guid.NewGuid(),
                now,
                job.Id),
            correlationId: job.Id.ToString(),
            cancellationToken: cancellationToken);

        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/run")]
    public async Task<ActionResult<PredictionResponse>> Run(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await db.PredictionJobs
            .Include(x => x.ModelDefinition)
            .Include(x => x.Candidates)
            .Include(x => x.Results)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (job.Status is not PredictionStatus.Queued
            and not PredictionStatus.Running)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid prediction status",
                Detail = "Only queued or running predictions can be executed.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var now = DateTimeOffset.UtcNow;

        if (job.Status == PredictionStatus.Queued)
        {
            job.Status = PredictionStatus.Running;
            job.StartedAtUtc = now;

            await publisher.PublishAsync(
                new PredictionStarted(
                    Guid.NewGuid(),
                    now,
                    job.Id),
                correlationId: job.Id.ToString(),
                cancellationToken: cancellationToken);
        }

        var ranked = scoringEngine.Score(
            job,
            job.ModelDefinition
                ?? throw new InvalidOperationException(
                    "Prediction model could not be loaded."));

        if (job.Results.Count > 0)
        {
            db.PredictionResults.RemoveRange(job.Results);
        }

        db.PredictionResults.AddRange(ranked);

        job.Status = PredictionStatus.Completed;
        job.CompletedAtUtc = now;
        job.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);

        job.Results.Clear();
        job.Results.AddRange(ranked);

        await publisher.PublishAsync(
            new PredictionCompleted(
                Guid.NewGuid(),
                now,
                job.Id,
                ranked.Count,
                ranked.Count(x => x.Prioritized)),
            correlationId: job.Id.ToString(),
            cancellationToken: cancellationToken);

        foreach (var result in ranked.Where(x => x.Prioritized))
        {
            await publisher.PublishAsync(
                new CandidatePrioritized(
                    Guid.NewGuid(),
                    now,
                    job.Id,
                    result.CompoundId,
                    result.PredictionScore,
                    result.Rank),
                correlationId: job.Id.ToString(),
                cancellationToken: cancellationToken);
        }

        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<PredictionResponse>> Fail(
        Guid id,
        FailPredictionRequest request,
        CancellationToken cancellationToken)
    {
        var job = await LoadJobAsync(id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (job.Status is PredictionStatus.Completed
            or PredictionStatus.Cancelled)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Invalid prediction status",
                Detail = "Completed or cancelled predictions cannot be failed.",
                Status = StatusCodes.Status409Conflict
            });
        }

        var now = DateTimeOffset.UtcNow;
        job.Status = PredictionStatus.Failed;
        job.FailureReason = request.Reason.Trim();
        job.CompletedAtUtc = now;
        job.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new PredictionFailed(
                Guid.NewGuid(),
                now,
                job.Id,
                job.FailureReason),
            correlationId: job.Id.ToString(),
            cancellationToken: cancellationToken);

        return Ok(job.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var job = await db.PredictionJobs
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (job is null)
        {
            return NotFound();
        }

        if (job.Status == PredictionStatus.Running)
        {
            return Conflict(new ProblemDetails
            {
                Title = "Prediction is running",
                Detail = "A running prediction cannot be deleted.",
                Status = StatusCodes.Status409Conflict
            });
        }

        db.PredictionJobs.Remove(job);
        await db.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    private Task<PredictionJob?> LoadJobAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return db.PredictionJobs
            .Include(x => x.Candidates)
            .Include(x => x.Results)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
    }

    private static PredictionCandidate CreateCandidate(
        CompoundProjection compound,
        decimal screeningScore,
        int screeningRank,
        bool screeningSelected)
    {
        return new PredictionCandidate
        {
            Id = Guid.NewGuid(),
            CompoundId = compound.CompoundId,
            CompoundName = compound.Name,
            ScreeningScore = screeningScore,
            ScreeningRank = screeningRank,
            ScreeningSelected = screeningSelected,
            MolecularWeight = compound.MolecularWeight
        };
    }
}
