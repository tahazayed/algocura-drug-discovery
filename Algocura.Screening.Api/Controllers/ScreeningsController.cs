using Algocura.Platform.Messaging.Publishing;
using Algocura.Screening.Api.Contracts;
using Algocura.Screening.Api.Data;
using Algocura.Screening.Api.Domain;
using Algocura.Screening.Api.Events;
using Algocura.Screening.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Screening.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/screenings")]
public sealed class ScreeningsController(ScreeningDbContext db, IEventPublisher publisher, ILogger<ScreeningsController> logger) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType<IReadOnlyCollection<ScreeningResponse>>(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyCollection<ScreeningResponse>>> GetAll([FromQuery] ScreeningStatus? status, [FromQuery] int skip = 0, [FromQuery] int take = 50, CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 200);
        var query = db.ScreeningJobs.AsNoTracking().Include(x => x.Candidates).Include(x => x.Results).AsQueryable();
        if (status.HasValue) query = query.Where(x => x.Status == status);
        var jobs = await query.OrderByDescending(x => x.CreatedAtUtc).Skip(skip).Take(take).ToListAsync(cancellationToken);
        return Ok(jobs.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType<ScreeningResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ScreeningResponse>> GetById(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        return job is null ? NotFound() : Ok(job.ToResponse());
    }

    [HttpPost]
    [ProducesResponseType<ScreeningResponse>(StatusCodes.Status201Created)]
    [ProducesResponseType<ValidationProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ScreeningResponse>> Create(CreateScreeningRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name)) ModelState.AddModelError(nameof(request.Name), "Name is required.");
        if (string.IsNullOrWhiteSpace(request.ScreeningMethod)) ModelState.AddModelError(nameof(request.ScreeningMethod), "Screening method is required.");
        if (request.CompoundIds.Count == 0) ModelState.AddModelError(nameof(request.CompoundIds), "At least one compound is required.");
        if (!ModelState.IsValid) return BadRequest(new ValidationProblemDetails(ModelState));

        var ids = request.CompoundIds.Distinct().ToArray();
        var compounds = await db.ScreeningCompounds.Where(x => ids.Contains(x.CompoundId)).ToListAsync(cancellationToken);
        var missing = ids.Except(compounds.Select(x => x.CompoundId)).ToArray();
        if (missing.Length > 0)
        {
            ModelState.AddModelError(nameof(request.CompoundIds), $"Unknown compound IDs: {string.Join(", ", missing)}");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;
        var job = new ScreeningJob
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ScreeningMethod = request.ScreeningMethod.Trim(),
            Status = ScreeningStatus.Queued,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            Candidates = compounds.Select(x => new ScreeningCandidate { Id = Guid.NewGuid(), CompoundId = x.CompoundId, CompoundName = x.Name, CanonicalSmiles = x.CanonicalSmiles, AddedAtUtc = now }).ToList()
        };
        db.ScreeningJobs.Add(job);
        await db.SaveChangesAsync(cancellationToken);
        await publisher.PublishAsync(
            new ScreeningCreated(
                Guid.NewGuid(),
                now,
                job.Id,
                job.Name,
                job.ScreeningMethod,
                job.Candidates.Count),
            cancellationToken: cancellationToken);
        logger.LogInformation("Created screening {ScreeningId} with {CandidateCount} candidates.", job.Id, job.Candidates.Count);
        return CreatedAtAction(nameof(GetById), new { id = job.Id }, job.ToResponse());
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ScreeningResponse>> Update(Guid id, UpdateScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        if (job is null) return NotFound();
        if (job.Status is not ScreeningStatus.Draft and not ScreeningStatus.Queued) return Conflict(new ProblemDetails { Title = "Screening cannot be edited", Detail = $"A screening in {job.Status} status cannot be edited." });
        if (string.IsNullOrWhiteSpace(request.Name) || string.IsNullOrWhiteSpace(request.ScreeningMethod)) return BadRequest();
        job.Name = request.Name.Trim();
        job.ScreeningMethod = request.ScreeningMethod.Trim();
        job.Description = request.Description?.Trim();
        job.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);
        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/start")]
    public async Task<ActionResult<ScreeningResponse>> Start(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        if (job is null) return NotFound();
        if (job.Status != ScreeningStatus.Queued) return InvalidTransition(job.Status, ScreeningStatus.Running);
        var now = DateTimeOffset.UtcNow;
        job.Status = ScreeningStatus.Running; job.StartedAtUtc = now; job.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        await publisher.PublishAsync(
            new ScreeningStarted(
                Guid.NewGuid(),
                now,
                job.Id),
            cancellationToken: cancellationToken);
        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<ScreeningResponse>> Complete(Guid id, CompleteScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        if (job is null) return NotFound();
        if (job.Status != ScreeningStatus.Running) return InvalidTransition(job.Status, ScreeningStatus.Completed);
        var candidateIds = job.Candidates.Select(x => x.CompoundId).ToHashSet();
        if (request.Results.Count == 0 || request.Results.Any(x => !candidateIds.Contains(x.CompoundId)) || request.Results.Select(x => x.CompoundId).Distinct().Count() != request.Results.Count)
            return BadRequest(new ProblemDetails { Title = "Invalid results", Detail = "Results must be unique and refer only to screening candidates." });
        var now = DateTimeOffset.UtcNow;
        var ranked = request.Results.OrderByDescending(x => x.Score).Select((x, index) => new ScreeningResult { Id = Guid.NewGuid(), ScreeningJobId = job.Id, CompoundId = x.CompoundId, Score = x.Score, Rank = index + 1, Selected = x.Selected, Notes = x.Notes?.Trim(), CreatedAtUtc = now }).ToList();
        if (job.Results.Count > 0)
        {
            db.ScreeningResults.RemoveRange(job.Results);
        }

        db.ScreeningResults.AddRange(ranked);

        job.Status = ScreeningStatus.Completed;
        job.CompletedAtUtc = now;
        job.UpdatedAtUtc = now;

        await db.SaveChangesAsync(cancellationToken);

        job.Results.Clear();
        job.Results.AddRange(ranked);
        await publisher.PublishAsync(
            new ScreeningCompleted(
                Guid.NewGuid(),
                now,
                job.Id,
                ranked.Count,
                ranked.Count(x => x.Selected)),
            cancellationToken: cancellationToken);
        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/fail")]
    public async Task<ActionResult<ScreeningResponse>> Fail(Guid id, FailScreeningRequest request, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        if (job is null) return NotFound();
        if (job.Status is not ScreeningStatus.Queued and not ScreeningStatus.Running) return InvalidTransition(job.Status, ScreeningStatus.Failed);
        if (string.IsNullOrWhiteSpace(request.Reason)) return BadRequest();
        var now = DateTimeOffset.UtcNow;
        job.Status = ScreeningStatus.Failed; job.FailureReason = request.Reason.Trim(); job.CompletedAtUtc = now; job.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        await publisher.PublishAsync(
            new ScreeningFailed(
                Guid.NewGuid(),
                now,
                job.Id,
                job.FailureReason),
            cancellationToken: cancellationToken);
        return Ok(job.ToResponse());
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<ActionResult<ScreeningResponse>> Cancel(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await LoadJobAsync(id, cancellationToken);
        if (job is null) return NotFound();
        if (job.Status is ScreeningStatus.Completed or ScreeningStatus.Failed or ScreeningStatus.Cancelled) return InvalidTransition(job.Status, ScreeningStatus.Cancelled);
        var now = DateTimeOffset.UtcNow;
        job.Status = ScreeningStatus.Cancelled; job.CompletedAtUtc = now; job.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);
        await publisher.PublishAsync(
            new ScreeningCancelled(
                Guid.NewGuid(),
                now,
                job.Id),
            cancellationToken: cancellationToken);
        return Ok(job.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken = default)
    {
        var job = await db.ScreeningJobs.FindAsync([id], cancellationToken);
        if (job is null) return NotFound();
        if (job.Status == ScreeningStatus.Running) return Conflict(new ProblemDetails { Title = "Running screening cannot be deleted." });
        db.ScreeningJobs.Remove(job);
        await db.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    private Task<ScreeningJob?> LoadJobAsync(Guid id, CancellationToken ct) => db.ScreeningJobs.Include(x => x.Candidates).Include(x => x.Results).SingleOrDefaultAsync(x => x.Id == id, ct);
    private ConflictObjectResult InvalidTransition(ScreeningStatus current, ScreeningStatus requested) => Conflict(new ProblemDetails { Title = "Invalid status transition", Detail = $"Cannot transition screening from {current} to {requested}." });
}