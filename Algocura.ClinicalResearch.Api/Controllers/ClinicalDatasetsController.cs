using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Data;
using Algocura.ClinicalResearch.Api.Domain;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Mapping;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ClinicalResearch.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/clinical-datasets")]
[Produces("application/json")]
public sealed class ClinicalDatasetsController(
    ClinicalResearchDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<ClinicalDatasetsController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ClinicalDatasetResponse>>> GetAll(
        [FromQuery] string? search,
        [FromQuery] ClinicalDatasetStatus? status,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(0, skip);
        take = Math.Clamp(take, 1, 200);

        var query = dbContext.ClinicalDatasets
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var term = search.Trim().ToLower();

            query = query.Where(x =>
                x.Name.ToLower().Contains(term) ||
                x.SourceName.ToLower().Contains(term) ||
                (x.StudyCode != null && x.StudyCode.ToLower().Contains(term)) ||
                (x.DiseaseArea != null && x.DiseaseArea.ToLower().Contains(term)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var rows = await query
            .OrderBy(x => x.Name)
            .Skip(skip)
            .Take(take)
            .Select(x => new ClinicalDatasetResponse(
                x.Id,
                x.Name,
                x.SourceName,
                x.SourceUri,
                x.StudyCode,
                x.DiseaseArea,
                x.Description,
                x.Status,
                x.Subjects.Count,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(rows);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ClinicalDatasetResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dataset = await dbContext.ClinicalDatasets
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ClinicalDatasetResponse(
                x.Id,
                x.Name,
                x.SourceName,
                x.SourceUri,
                x.StudyCode,
                x.DiseaseArea,
                x.Description,
                x.Status,
                x.Subjects.Count,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return dataset is null
            ? Problem(
                statusCode: 404,
                title: "Clinical dataset not found",
                detail: $"No dataset with ID '{id}' was found.")
            : Ok(dataset);
    }

    [HttpPost]
    public async Task<ActionResult<ClinicalDatasetResponse>> Create(
        CreateClinicalDatasetRequest request,
        CancellationToken cancellationToken)
    {
        var now = DateTimeOffset.UtcNow;

        var dataset = new ClinicalDataset
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            SourceName = request.SourceName.Trim(),
            SourceUri = request.SourceUri?.Trim(),
            StudyCode = request.StudyCode?.Trim(),
            DiseaseArea = request.DiseaseArea?.Trim(),
            Description = request.Description?.Trim(),
            Status = request.Status,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        dbContext.ClinicalDatasets.Add(dataset);

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await eventPublisher.PublishAsync(
                new ClinicalDatasetRegistered
                {
                    ClinicalDatasetId = dataset.Id,
                    Name = dataset.Name,
                    SourceName = dataset.SourceName,
                    SourceUri = dataset.SourceUri,
                    StudyCode = dataset.StudyCode,
                    DiseaseArea = dataset.DiseaseArea,
                    Status = dataset.Status
                },
                HttpContext?.TraceIdentifier,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Clinical dataset {ClinicalDatasetId} was saved successfully, but the ClinicalDatasetRegistered event could not be published.",
                dataset.Id);
        }

        logger.LogInformation(
            "Registered clinical dataset {ClinicalDatasetId} from {SourceName}",
            dataset.Id,
            dataset.SourceName);

        return CreatedAtAction(
            nameof(GetById),
            new { id = dataset.Id },
            dataset.ToResponse(0));
    }

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ClinicalDatasetResponse>> Update(
        Guid id,
        UpdateClinicalDatasetRequest request,
        CancellationToken cancellationToken)
    {
        var dataset = await dbContext.ClinicalDatasets
            .Include(x => x.Subjects)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (dataset is null)
        {
            return Problem(
                statusCode: 404,
                title: "Clinical dataset not found");
        }

        var previousStatus = dataset.Status;

        dataset.Name = request.Name.Trim();
        dataset.SourceName = request.SourceName.Trim();
        dataset.SourceUri = request.SourceUri?.Trim();
        dataset.StudyCode = request.StudyCode?.Trim();
        dataset.DiseaseArea = request.DiseaseArea?.Trim();
        dataset.Description = request.Description?.Trim();
        dataset.Status = request.Status;
        dataset.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await eventPublisher.PublishAsync(
                new ClinicalDatasetUpdated
                {
                    ClinicalDatasetId = dataset.Id,
                    Name = dataset.Name,
                    SourceName = dataset.SourceName,
                    StudyCode = dataset.StudyCode,
                    DiseaseArea = dataset.DiseaseArea,
                    Status = dataset.Status
                },
                HttpContext?.TraceIdentifier,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Clinical dataset {ClinicalDatasetId} was updated successfully, but the ClinicalDatasetUpdated event could not be published.",
                dataset.Id);
        }

        if (previousStatus != dataset.Status)
        {
            try
            {
                await eventPublisher.PublishAsync(
                    new ClinicalDatasetStatusChanged
                    {
                        ClinicalDatasetId = dataset.Id,
                        PreviousStatus = previousStatus,
                        CurrentStatus = dataset.Status
                    },
                    HttpContext?.TraceIdentifier,
                    cancellationToken);
            }
            catch (Exception ex)
            {
                logger.LogError(
                    ex,
                    "Clinical dataset {ClinicalDatasetId} status changed from {PreviousStatus} to {CurrentStatus}, but the ClinicalDatasetStatusChanged event could not be published.",
                    dataset.Id,
                    previousStatus,
                    dataset.Status);
            }
        }

        return Ok(
            dataset.ToResponse(
                dataset.Subjects.Count));
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var dataset = await dbContext.ClinicalDatasets
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (dataset is null)
        {
            return Problem(
                statusCode: 404,
                title: "Clinical dataset not found");
        }

        dbContext.ClinicalDatasets.Remove(dataset);

        await dbContext.SaveChangesAsync(cancellationToken);

        try
        {
            await eventPublisher.PublishAsync(
                new ClinicalDatasetDeleted
                {
                    ClinicalDatasetId = dataset.Id,
                    Name = dataset.Name
                },
                HttpContext?.TraceIdentifier,
                cancellationToken);
        }
        catch (Exception ex)
        {
            logger.LogError(
                ex,
                "Clinical dataset {ClinicalDatasetId} was deleted successfully, but the ClinicalDatasetDeleted event could not be published.",
                dataset.Id);
        }

        return NoContent();
    }
}
