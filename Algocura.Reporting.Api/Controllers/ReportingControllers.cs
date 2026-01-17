using Algocura.Reporting.Api.Contracts;
using Algocura.Reporting.Api.Data;
using Algocura.Reporting.Api.Domain;
using Algocura.Reporting.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Reporting.Api.Controllers;

[ApiController]
[Route("api/v1/report-definitions")]
public sealed class ReportDefinitionsController(
    ReportingDbContext db,
    IReportExecutionService executor)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReportDefinitionResponse>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var items = await db.ReportDefinitions
            .AsNoTracking()
            .OrderBy(x => x.Name)
            .Select(x => new ReportDefinitionResponse(
                x.Id,
                x.Name,
                x.Description,
                x.ReportType,
                x.Status,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(items);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportDefinitionResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var item = await db.ReportDefinitions
            .AsNoTracking()
            .Where(x => x.Id == id)
            .Select(x => new ReportDefinitionResponse(
                x.Id,
                x.Name,
                x.Description,
                x.ReportType,
                x.Status,
                x.CreatedAtUtc,
                x.UpdatedAtUtc))
            .SingleOrDefaultAsync(cancellationToken);

        return item is null ? NotFound() : Ok(item);
    }

    [HttpPost]
    public async Task<ActionResult<ReportDefinitionResponse>> Create(
        CreateReportDefinitionRequest request,
        CancellationToken cancellationToken = default)
    {
        var now = DateTimeOffset.UtcNow;

        var item = new ReportDefinition
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            ReportType = request.ReportType.Trim(),
            DefaultParametersJson = request.DefaultParametersJson,
            Status = ReportDefinitionStatus.Active,
            OwnerUserId = SystemUsers.SystemUserId,
            ApprovedByUserId = SystemUsers.SystemUserId,
            ApprovedAtUtc = now,
            CreatedByUserId = SystemUsers.SystemUserId,
            UpdatedByUserId = SystemUsers.SystemUserId,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        await db.ReportDefinitions.AddAsync(item, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);

        var response = new ReportDefinitionResponse(
            item.Id,
            item.Name,
            item.Description,
            item.ReportType,
            item.Status,
            item.CreatedAtUtc,
            item.UpdatedAtUtc);

        return CreatedAtAction(
            nameof(GetById),
            new { id = item.Id },
            response);
    }

    [HttpPost("{id:guid}/runs")]
    public async Task<ActionResult<ReportRunResponse>> Run(
        Guid id,
        RunReportRequest request,
        CancellationToken cancellationToken = default)
    {
        var definition = await db.ReportDefinitions
            .SingleOrDefaultAsync(
                x =>
                    x.Id == id &&
                    x.Status == ReportDefinitionStatus.Active,
                cancellationToken);

        if (definition is null)
        {
            return NotFound();
        }

        var run = await executor.ExecuteAsync(
            definition,
            string.IsNullOrWhiteSpace(request.Name)
                ? $"{definition.Name} - {DateTimeOffset.UtcNow:yyyy-MM-dd HH:mm:ss}"
                : request.Name.Trim(),
            request.ParametersJson ??
                definition.DefaultParametersJson,
            cancellationToken);

        return Ok(Map(run));
    }

    private static ReportRunResponse Map(ReportRun run) =>
        new(
            run.Id,
            run.ReportDefinitionId,
            run.Name,
            run.Status,
            run.FailureReason,
            run.ResultLocation,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            run.Metrics.Select(x => new ReportMetricResponse(
                x.Name,
                x.Value,
                x.ValueType,
                x.Unit)).ToArray());
}

[ApiController]
[Route("api/v1/report-runs")]
public sealed class ReportRunsController(
    ReportingDbContext db)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<ReportRunResponse>>> GetAll(
        CancellationToken cancellationToken = default)
    {
        var runs = await db.ReportRuns
            .AsNoTracking()
            .Include(x => x.Metrics)
            .OrderByDescending(x => x.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return Ok(runs.Select(Map).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ReportRunResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken = default)
    {
        var run = await db.ReportRuns
            .AsNoTracking()
            .Include(x => x.Metrics)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return run is null ? NotFound() : Ok(Map(run));
    }

    private static ReportRunResponse Map(ReportRun run) =>
        new(
            run.Id,
            run.ReportDefinitionId,
            run.Name,
            run.Status,
            run.FailureReason,
            run.ResultLocation,
            run.StartedAtUtc,
            run.CompletedAtUtc,
            run.Metrics.Select(x => new ReportMetricResponse(
                x.Name,
                x.Value,
                x.ValueType,
                x.Unit)).ToArray());
}

[ApiController]
[Route("api/v1/reporting/summary")]
public sealed class ReportingSummaryController(
    ReportingDbContext db)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ReportingSummaryResponse>> Get(
        CancellationToken cancellationToken = default)
    {
        var groups = await db.ReportingProjections
            .AsNoTracking()
            .GroupBy(x => x.ProjectionType)
            .Select(x => new
            {
                Type = x.Key.ToString(),
                Count = x.Count()
            })
            .ToListAsync(cancellationToken);

        var counts = groups.ToDictionary(
            x => x.Type,
            x => x.Count,
            StringComparer.OrdinalIgnoreCase);

        var definitions = await db.ReportDefinitions.CountAsync(
            x => x.Status == ReportDefinitionStatus.Active,
            cancellationToken);

        var runs = await db.ReportRuns.CountAsync(
            x => x.Status == ReportRunStatus.Completed,
            cancellationToken);

        return Ok(new ReportingSummaryResponse(
            counts.Values.Sum(),
            counts,
            definitions,
            runs));
    }
}
