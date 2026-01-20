using Algocura.AssayManagement.Api.Contracts;
using Algocura.AssayManagement.Api.Data;
using Algocura.AssayManagement.Api.Domain;
using Algocura.AssayManagement.Api.Events;
using Algocura.AssayManagement.Api.Mapping;
using Algocura.AssayManagement.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.AssayManagement.Api.Controllers;

[ApiController]
[Route("api/v1/assays")]
public sealed class AssaysController(
    AssayManagementDbContext dbContext,
    IAssayResultCalculator resultCalculator,
    IEventPublisher eventPublisher,
    ILogger<AssaysController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<AssayResponse>>> GetAll(
            AssayStatus? status,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        var query = dbContext.Assays
            .AsNoTracking()
            .Include(x => x.Measurements)
            .Include(x => x.Results)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(
                x => x.Status == status.Value);
        }

        var assays = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

        return Ok(
            assays
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<AssayResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assay = await LoadAssayAsync(
            id,
            cancellationToken);

        return assay is null
            ? NotFound()
            : Ok(assay.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<AssayResponse>> Create(
        CreateAssayRequest request,
        CancellationToken cancellationToken)
    {
        var screeningExists =
            await dbContext.Screenings
                .AnyAsync(
                    x =>
                        x.ScreeningId ==
                        request.ScreeningId,
                    cancellationToken);

        if (!screeningExists)
        {
            ModelState.AddModelError(
                nameof(request.ScreeningId),
                "The completed screening was not found.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;

        var assay = new Assay
        {
            Id = Guid.NewGuid(),
            ScreeningId =
                request.ScreeningId,
            Name =
                request.Name.Trim(),
            AssayType =
                request.AssayType,
            BiologicalTarget =
                request.BiologicalTarget?.Trim(),
            Protocol =
                request.Protocol?.Trim(),
            MeasurementUnit =
                request.MeasurementUnit.Trim(),
            ActivityThreshold =
                request.ActivityThreshold,
            Status =
                AssayStatus.Draft,
            QualityControlStatus =
                QualityControlStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.Assays.Add(assay);

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            new AssayCreated(
                Guid.NewGuid(),
                now,
                assay.Id,
                assay.ScreeningId,
                assay.Name,
                assay.AssayType.ToString()),
            assay.Id.ToString(),
            cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = assay.Id },
            assay.ToResponse());
    }

    [HttpPost("{id:guid}/measurements")]
    public async Task<ActionResult<AssayMeasurementResponse>>
        AddMeasurement(
            Guid id,
            AddAssayMeasurementRequest request,
            CancellationToken cancellationToken)
    {
        var assay = await dbContext.Assays
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (assay is null)
        {
            return NotFound();
        }

        if (assay.Status is
            AssayStatus.Completed or
            AssayStatus.Published)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assay is finalized",
                    Detail =
                        "Measurements cannot be added to a completed or published assay.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var compoundExists =
            await dbContext.ScreeningResults
                .AnyAsync(
                    x =>
                        x.ScreeningId ==
                        assay.ScreeningId &&
                        x.CompoundId ==
                        request.CompoundId,
                    cancellationToken);

        if (!compoundExists &&
            !request.IsControl)
        {
            ModelState.AddModelError(
                nameof(request.CompoundId),
                "The compound does not belong to the assay screening.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        var measurement = new AssayMeasurement
        {
            Id = Guid.NewGuid(),
            AssayId =
                assay.Id,
            CompoundId =
                request.CompoundId,
            MeasurementType =
                request.MeasurementType,
            Concentration =
                request.Concentration,
            ConcentrationUnit =
                request.ConcentrationUnit.Trim(),
            ReplicateNumber =
                request.ReplicateNumber,
            Value =
                request.Value,
            IsControl =
                request.IsControl,
            RecordedAtUtc =
                DateTimeOffset.UtcNow
        };

        dbContext.AssayMeasurements.Add(
            measurement);

        assay.Status =
            AssayStatus.InProgress;

        assay.UpdatedAtUtc =
            DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        return Created(
            $"/api/v1/assays/{assay.Id}/measurements/{measurement.Id}",
            measurement.ToResponse());
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<AssayResponse>> Complete(
        Guid id,
        CompleteAssayRequest request,
        CancellationToken cancellationToken)
    {
        var assay = await LoadAssayAsync(
            id,
            cancellationToken);

        if (assay is null)
        {
            return NotFound();
        }

        if (assay.Status is
            AssayStatus.Completed or
            AssayStatus.Published)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assay is already finalized",
                    Detail =
                        "The assay has already been completed or published.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        if (!assay.Measurements.Any(
                x => !x.IsControl))
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "No compound measurements",
                    Detail =
                        "At least one non-control measurement is required.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var existingResults =
            await dbContext.AssayResults
                .Where(
                    x => x.AssayId == assay.Id)
                .ToListAsync(cancellationToken);

        if (existingResults.Count > 0)
        {
            dbContext.AssayResults.RemoveRange(
                existingResults);

            await dbContext.SaveChangesAsync(
                cancellationToken);
        }

        var results =
            resultCalculator.Calculate(
                assay,
                assay.Measurements);

        if (results.Count > 0)
        {
            dbContext.AssayResults.AddRange(
                results);
        }

        var now = DateTimeOffset.UtcNow;

        assay.ZPrimeFactor =
            request.ZPrimeFactor;

        assay.QualityControlStatus =
            request.QualityControlStatus;

        assay.QualityControlNotes =
            request.QualityControlNotes?.Trim();

        assay.Status =
            request.QualityControlStatus ==
            QualityControlStatus.Failed
                ? AssayStatus.Rejected
                : AssayStatus.Completed;

        assay.CompletedAtUtc = now;
        assay.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            new AssayCompleted(
                Guid.NewGuid(),
                now,
                assay.Id,
                assay.ScreeningId,
                results.Count,
                assay.QualityControlStatus.ToString()),
            assay.Id.ToString(),
            cancellationToken);

        dbContext.ChangeTracker.Clear();

        assay = await LoadAssayAsync(
            id,
            cancellationToken)
            ?? throw new InvalidOperationException(
                "The assay could not be reloaded.");

        logger.LogInformation(
            "Completed assay {AssayId} with {ResultCount} results.",
            assay.Id,
            assay.Results.Count);

        return Ok(assay.ToResponse());
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<AssayResponse>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        var assay = await LoadAssayAsync(
            id,
            cancellationToken);

        if (assay is null)
        {
            return NotFound();
        }

        if (assay.Status !=
            AssayStatus.Completed)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Assay is not completed",
                    Detail =
                        "Only completed assays can be published.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var now = DateTimeOffset.UtcNow;

        assay.Status =
            AssayStatus.Published;

        assay.PublishedAtUtc = now;
        assay.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(
            cancellationToken);

        await eventPublisher.PublishAsync(
            CreateSnapshot(
                assay,
                now),
            assay.Id.ToString(),
            cancellationToken);

        return Ok(assay.ToResponse());
    }

    private Task<Assay?> LoadAssayAsync(
        Guid id,
        CancellationToken cancellationToken)
    {
        return dbContext.Assays
            .Include(x => x.Measurements)
            .Include(x => x.Results)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);
    }

    internal static AssaySnapshot CreateSnapshot(
        Assay assay,
        DateTimeOffset occurredAtUtc)
    {
        return new AssaySnapshot(
            Guid.NewGuid(),
            occurredAtUtc,
            assay.Id,
            assay.ScreeningId,
            assay.Name,
            assay.AssayType.ToString(),
            assay.BiologicalTarget,
            assay.MeasurementUnit,
            assay.ActivityThreshold,
            assay.QualityControlStatus.ToString(),
            assay.ZPrimeFactor,
            assay.Results
                .OrderByDescending(x => x.MeanValue)
                .Select(
                    result =>
                        new AssaySnapshotResult(
                            result.CompoundId,
                            result.MeanValue,
                            result.StandardDeviation,
                            result.ReplicateCount,
                            result.Ic50,
                            result.Ec50,
                            result.Ki,
                            result.Kd,
                            result.ConfidenceScore,
                            result.Outcome.ToString(),
                            result.SelectedForAi))
                .ToArray());
    }
}

