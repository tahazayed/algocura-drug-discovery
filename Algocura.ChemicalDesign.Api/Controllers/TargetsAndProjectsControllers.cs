using Algocura.ChemicalDesign.Api.Contracts;
using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Mapping;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Controllers;

[ApiController]
[Route("api/v1/selected-targets")]
public sealed class SelectedTargetsController(ChemicalDesignDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<SelectedTargetResponse>>> GetAll(
        string? disease, string? gene, int skip = 0, int take = 50,
        CancellationToken cancellationToken = default)
    {
        var query = db.SelectedTargets.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(disease))
            query = query.Where(x => x.DiseaseName.Contains(disease.Trim()));

        if (!string.IsNullOrWhiteSpace(gene))
        {
            var normalized = gene.Trim().ToUpperInvariant();
            query = query.Where(x => x.GeneSymbol == normalized);
        }

        var items = await query
            .OrderByDescending(x => x.TargetAssessmentScore)
            .Skip(Math.Max(skip, 0))
            .Take(Math.Clamp(take, 1, 200))
            .ToListAsync(cancellationToken);

        return Ok(items.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SelectedTargetResponse>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var item = await db.SelectedTargets.AsNoTracking()
            .SingleOrDefaultAsync(x => x.TargetAssessmentId == id, cancellationToken);

        return item is null ? NotFound() : Ok(item.ToResponse());
    }
}

[ApiController]
[Route("api/v1/design-projects")]
public sealed class DesignProjectsController(
    ChemicalDesignDbContext db,
    IEventPublisher publisher,
    ILogger<DesignProjectsController> logger) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<DesignProjectResponse>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var project = await LoadAsync(id, cancellationToken);
        return project is null ? NotFound() : Ok(project.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<DesignProjectResponse>> Create(
        CreateDesignProjectRequest request,
        CancellationToken cancellationToken)
    {
        var target = await db.SelectedTargets.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.TargetAssessmentId == request.TargetAssessmentId,
                cancellationToken);

        if (target is null)
        {
            ModelState.AddModelError(
                nameof(request.TargetAssessmentId),
                "The selected target was not found.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;
        var project = new ChemicalDesignProject
        {
            Id = Guid.NewGuid(),
            TargetAssessmentId = target.TargetAssessmentId,
            Name = request.Name.Trim(),
            Objective = request.Objective?.Trim(),
            DesiredModulation = target.DesiredModulation,
            Status = DesignProjectStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.DesignProjects.Add(project);
        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new ChemicalDesignProjectCreated(
                Guid.NewGuid(), now, project.Id, target.TargetAssessmentId,
                project.Name, target.GeneSymbol),
            project.Id.ToString(),
            cancellationToken);

        logger.LogInformation(
            "Created chemical design project {DesignProjectId}.",
            project.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = project.Id },
            project.ToResponse());
    }

    [HttpPost("{id:guid}/query-structures")]
    public async Task<ActionResult<QueryStructureResponse>> AddQueryStructure(
        Guid id,
        CreateQueryStructureRequest request,
        CancellationToken cancellationToken)
    {
        var project = await db.DesignProjects.SingleOrDefaultAsync(
            x => x.Id == id, cancellationToken);

        if (project is null) return NotFound();

        if (project.Status is DesignProjectStatus.Completed or DesignProjectStatus.Cancelled)
            return Conflict(new ProblemDetails
            {
                Title = "Project is finalized",
                Detail = "Query structures cannot be added to a finalized project.",
                Status = StatusCodes.Status409Conflict
            });

        var query = new QueryStructure
        {
            Id = Guid.NewGuid(),
            DesignProjectId = project.Id,
            Name = request.Name.Trim(),
            Structure = request.Structure.Trim(),
            StructureFormat = request.StructureFormat.Trim(),
            SearchPurpose = request.SearchPurpose?.Trim(),
            CreatedAtUtc = DateTimeOffset.UtcNow
        };

        db.QueryStructures.Add(query);
        project.Status = DesignProjectStatus.InProgress;
        project.UpdatedAtUtc = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/v1/design-projects/{project.Id}/query-structures/{query.Id}",
            query.ToResponse());
    }

    [HttpPost("{id:guid}/complete")]
    public async Task<ActionResult<DesignProjectResponse>> Complete(
        Guid id, CancellationToken cancellationToken)
    {
        var project = await LoadAsync(id, cancellationToken);
        if (project is null) return NotFound();

        if (!project.Candidates.Any(
                x => x.Status == CandidateStructureStatus.SelectedForRegistration))
            return Conflict(new ProblemDetails
            {
                Title = "No selected candidate",
                Detail = "Select at least one candidate for registration first.",
                Status = StatusCodes.Status409Conflict
            });

        project.Status = DesignProjectStatus.Completed;
        project.CompletedAtUtc = DateTimeOffset.UtcNow;
        project.UpdatedAtUtc = project.CompletedAtUtc.Value;
        await db.SaveChangesAsync(cancellationToken);

        return Ok(project.ToResponse());
    }

    private Task<ChemicalDesignProject?> LoadAsync(
        Guid id, CancellationToken cancellationToken) =>
        db.DesignProjects
            .Include(x => x.QueryStructures)
            .Include(x => x.SearchJobs).ThenInclude(x => x.Candidates)
            .Include(x => x.Candidates)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);
}
