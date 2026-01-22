using Algocura.ChemicalDesign.Api.Contracts;
using Algocura.ChemicalDesign.Api.Data;
using Algocura.ChemicalDesign.Api.Domain;
using Algocura.ChemicalDesign.Api.Events;
using Algocura.ChemicalDesign.Api.Mapping;
using Algocura.ChemicalDesign.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.ChemicalDesign.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/structure-searches")]
public sealed class StructureSearchesController(
    ChemicalDesignDbContext db,
    IStructureSearchEngine engine,
    IEventPublisher publisher) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<StructureSearchResponse>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var item = await db.StructureSearchJobs.AsNoTracking()
            .Include(x => x.Candidates)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return item is null ? NotFound() : Ok(item.ToResponse());
    }

    [HttpPost("/api/v1/design-projects/{projectId:guid}/structure-searches")]
    public async Task<ActionResult<StructureSearchResponse>> Create(
        Guid projectId,
        CreateStructureSearchRequest request,
        CancellationToken cancellationToken)
    {
        var project = await db.DesignProjects.SingleOrDefaultAsync(
            x => x.Id == projectId, cancellationToken);

        if (project is null) return NotFound();

        var query = await db.QueryStructures.AsNoTracking()
            .SingleOrDefaultAsync(
                x => x.Id == request.QueryStructureId &&
                     x.DesignProjectId == projectId,
                cancellationToken);

        if (query is null)
        {
            ModelState.AddModelError(
                nameof(request.QueryStructureId),
                "The query structure was not found in this project.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        if (request.SimilarityThreshold is < 0m or > 1m)
        {
            ModelState.AddModelError(
                nameof(request.SimilarityThreshold),
                "SimilarityThreshold must be between 0 and 1.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;
        var search = new StructureSearchJob
        {
            Id = Guid.NewGuid(),
            DesignProjectId = projectId,
            QueryStructureId = query.Id,
            SearchType = request.SearchType,
            SimilarityThreshold = request.SimilarityThreshold,
            MaximumResults = Math.Clamp(request.MaximumResults, 1, 500),
            Provider = string.IsNullOrWhiteSpace(request.Provider)
                ? "Prototype"
                : request.Provider.Trim(),
            Status = StructureSearchStatus.Queued,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        db.StructureSearchJobs.Add(search);
        project.Status = DesignProjectStatus.InProgress;
        project.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(
            nameof(GetById),
            new { id = search.Id },
            search.ToResponse());
    }

    [HttpPost("{id:guid}/run")]
    public async Task<ActionResult<StructureSearchResponse>> Run(
    Guid id,
    CancellationToken cancellationToken)
    {
        var search = await db.StructureSearchJobs
            .Include(x => x.Candidates)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (search is null)
        {
            return NotFound();
        }

        if (search.Status is
            StructureSearchStatus.Running or
            StructureSearchStatus.Completed)
        {
            return Conflict(
                new ProblemDetails
                {
                    Title = "Search cannot be executed",
                    Detail =
                        "A running or completed search cannot be executed again.",
                    Status =
                        StatusCodes.Status409Conflict
                });
        }

        var query = await db.QueryStructures
            .AsNoTracking()
            .SingleAsync(
                x => x.Id == search.QueryStructureId,
                cancellationToken);

        var library = await db.ChemicalLibrary
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        search.Status =
            StructureSearchStatus.Running;

        search.StartedAtUtc =
            DateTimeOffset.UtcNow;

        search.UpdatedAtUtc =
            search.StartedAtUtc.Value;

        await db.SaveChangesAsync(
            cancellationToken);

        try
        {
            var matches = engine.Search(
                query,
                search.SearchType,
                search.SimilarityThreshold,
                search.MaximumResults,
                library);

            var candidates = matches
                .Select(
                    match =>
                        new CandidateStructure
                        {
                            Id = Guid.NewGuid(),

                            DesignProjectId =
                                search.DesignProjectId,

                            StructureSearchJobId =
                                search.Id,

                            Name =
                                match.Entry.Name,

                            CanonicalSmiles =
                                match.Entry.CanonicalSmiles,

                            InchiKey =
                                match.Entry.InchiKey,

                            MolecularFormula =
                                match.Entry.MolecularFormula,

                            MolecularWeight =
                                match.Entry.MolecularWeight,

                            MatchScore =
                                match.MatchScore,

                            SourceReference =
                                match.Entry.SourceReference,

                            Status =
                                CandidateStructureStatus.Proposed,

                            CreatedAtUtc =
                                DateTimeOffset.UtcNow,

                            UpdatedAtUtc =
                                DateTimeOffset.UtcNow
                        })
                .ToArray();

            if (candidates.Length > 0)
            {
                db.CandidateStructures.AddRange(
                    candidates);
            }

            search.Status =
                StructureSearchStatus.Completed;

            search.CompletedAtUtc =
                DateTimeOffset.UtcNow;

            search.UpdatedAtUtc =
                search.CompletedAtUtc.Value;

            await db.SaveChangesAsync(
                cancellationToken);

            await publisher.PublishAsync(
                new StructureSearchCompleted(
                    Guid.NewGuid(),
                    search.CompletedAtUtc.Value,
                    search.Id,
                    search.DesignProjectId,
                    search.SearchType.ToString(),
                    candidates.Length),
                search.Id.ToString(),
                cancellationToken);

            return Ok(search.ToResponse());
        }
        catch (OperationCanceledException)
            when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            search.Status =
                StructureSearchStatus.Failed;

            search.FailureReason =
                exception.Message;

            search.UpdatedAtUtc =
                DateTimeOffset.UtcNow;

            await db.SaveChangesAsync(
                cancellationToken);

            throw;
        }
    }
}

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/candidates")]
public sealed class CandidatesController(
    ChemicalDesignDbContext db,
    IEventPublisher publisher) : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<CandidateStructureResponse>> GetById(
        Guid id, CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateStructures.AsNoTracking()
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return candidate is null ? NotFound() : Ok(candidate.ToResponse());
    }

    [HttpPost("{id:guid}/select-for-registration")]
    public async Task<ActionResult<CandidateStructureResponse>> Select(
        Guid id,
        SelectCandidateRequest request,
        CancellationToken cancellationToken)
    {
        var candidate = await db.CandidateStructures
            .Include(x => x.DesignProject)
            .ThenInclude(x => x!.SelectedTarget)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        if (candidate is null) return NotFound();

        var project = candidate.DesignProject
            ?? throw new InvalidOperationException("Design project was not loaded.");
        var target = project.SelectedTarget
            ?? throw new InvalidOperationException("Selected target was not loaded.");

        var now = DateTimeOffset.UtcNow;
        candidate.Status = CandidateStructureStatus.SelectedForRegistration;
        candidate.SelectionRationale = request.SelectionRationale.Trim();
        candidate.SelectedAtUtc = now;
        candidate.UpdatedAtUtc = now;
        await db.SaveChangesAsync(cancellationToken);

        await publisher.PublishAsync(
            new CandidateSelectedForRegistration(
                Guid.NewGuid(), now, candidate.Id, project.Id,
                target.TargetAssessmentId, candidate.Name,
                candidate.CanonicalSmiles, candidate.InchiKey,
                candidate.MolecularFormula, candidate.MolecularWeight,
                candidate.SelectionRationale),
            candidate.Id.ToString(),
            cancellationToken);

        return Ok(candidate.ToResponse());
    }

    [HttpPost("{id:guid}/synthesis-routes")]
    public async Task<ActionResult<SynthesisRouteResponse>> AddRoute(
        Guid id,
        CreateSynthesisRouteRequest request,
        CancellationToken cancellationToken)
    {
        if (!await db.CandidateStructures.AnyAsync(x => x.Id == id, cancellationToken))
            return NotFound();

        if (request.Steps.Count == 0)
        {
            ModelState.AddModelError(nameof(request.Steps),
                "At least one reaction step is required.");
            return BadRequest(new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;
        var route = new SynthesisRoute
        {
            Id = Guid.NewGuid(),
            CandidateStructureId = id,
            Name = request.Name.Trim(),
            Description = request.Description?.Trim(),
            EstimatedYieldPercent = request.EstimatedYieldPercent,
            StepCount = request.Steps.Count,
            Status = SynthesisRouteStatus.Draft,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var step in request.Steps.OrderBy(x => x.StepNumber))
        {
            route.Steps.Add(new ReactionStep
            {
                Id = Guid.NewGuid(),
                SynthesisRouteId = route.Id,
                StepNumber = step.StepNumber,
                ReactionName = step.ReactionName.Trim(),
                Reactants = step.Reactants?.Trim(),
                Reagents = step.Reagents?.Trim(),
                Conditions = step.Conditions?.Trim(),
                ProductSmiles = step.ProductSmiles?.Trim(),
                EstimatedYieldPercent = step.EstimatedYieldPercent
            });
        }

        db.SynthesisRoutes.Add(route);
        await db.SaveChangesAsync(cancellationToken);

        return Created(
            $"/api/v1/candidates/{id}/synthesis-routes/{route.Id}",
            route.ToResponse());
    }
}
