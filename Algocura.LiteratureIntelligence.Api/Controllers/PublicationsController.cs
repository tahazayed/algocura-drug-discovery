using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.LiteratureIntelligence.Api.Mapping;
using Algocura.LiteratureIntelligence.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/publications")]
public sealed class PublicationsController(
    LiteratureIntelligenceDbContext dbContext,
    ILiteratureAnalysisService analysisService,
    IEventPublisher eventPublisher,
    ILogger<PublicationsController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyCollection<PublicationResponse>>> GetAll(
        string? search,
        PublicationStatus? status,
        int skip = 0,
        int take = 50,
        CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        var query = dbContext.Publications
            .AsNoTracking()
            .Include(x => x.Authors)
            .Include(x => x.EvidenceStatements)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var value = search.Trim();

            query = query.Where(x =>
                x.Title.Contains(value) ||
                x.Abstract.Contains(value) ||
                (x.Journal != null && x.Journal.Contains(value)) ||
                (x.ExternalId != null && x.ExternalId.Contains(value)));
        }

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var publications = await query
            .OrderByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(publications.Select(x => x.ToResponse()).ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<PublicationResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var publication = await dbContext.Publications
            .AsNoTracking()
            .Include(x => x.Authors)
            .Include(x => x.EvidenceStatements)
            .SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

        return publication is null
            ? NotFound()
            : Ok(publication.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<PublicationResponse>> Create(
        CreatePublicationRequest request,
        CancellationToken cancellationToken)
    {
        if (!string.IsNullOrWhiteSpace(request.ExternalId))
        {
            var exists = await dbContext.Publications.AnyAsync(
                x => x.ExternalId == request.ExternalId.Trim(),
                cancellationToken);

            if (exists)
            {
                ModelState.AddModelError(
                    nameof(request.ExternalId),
                    "A publication with this external ID already exists.");

                return BadRequest(new ValidationProblemDetails(ModelState));
            }
        }

        var now = DateTimeOffset.UtcNow;

        var publication = new Publication
        {
            Id = Guid.NewGuid(),
            Title = request.Title.Trim(),
            Abstract = request.Abstract.Trim(),
            Journal = request.Journal?.Trim(),
            ExternalId = request.ExternalId?.Trim(),
            SourceUrl = request.SourceUrl?.Trim(),
            PublishedDate = request.PublishedDate,
            Status = PublicationStatus.PendingAnalysis,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        var authors = request.Authors?
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(x => x.Trim())
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray() ?? [];

        for (var i = 0; i < authors.Length; i++)
        {
            publication.Authors.Add(new PublicationAuthor
            {
                Id = Guid.NewGuid(),
                PublicationId = publication.Id,
                Name = authors[i],
                DisplayOrder = i + 1
            });
        }

        dbContext.Publications.Add(publication);

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new PublicationRegistered(
                Guid.NewGuid(),
                now,
                publication.Id,
                publication.Title,
                publication.ExternalId),
            publication.Id.ToString(),
            cancellationToken);

        logger.LogInformation(
            "Registered publication {PublicationId}.",
            publication.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = publication.Id },
            publication.ToResponse());
    }

    [HttpPost("{id:guid}/analyze")]
    public async Task<ActionResult<PublicationResponse>> Analyze(
        Guid id,
        AnalyzePublicationRequest request,
        CancellationToken cancellationToken)
    {
        var publication = await dbContext.Publications
            .Include(x => x.Authors)
            .Include(x => x.EvidenceStatements)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (publication is null)
        {
            return NotFound();
        }

        // Remove previous evidence
        if (publication.EvidenceStatements.Any())
        {
            dbContext.EvidenceStatements.RemoveRange(
                publication.EvidenceStatements.ToList());
        }

        // Generate new evidence
        var statements = analysisService.Analyze(
            publication,
            request.DiseaseName,
            request.CandidateGeneSymbols);

        if (statements.Count > 0)
        {
            dbContext.EvidenceStatements.AddRange(
                statements);
        }

        var now = DateTimeOffset.UtcNow;

        publication.Status = PublicationStatus.Analyzed;
        publication.AnalyzedAtUtc = now;
        publication.UpdatedAtUtc = now;

        await dbContext.SaveChangesAsync(cancellationToken);

        foreach (var statement in statements)
        {
            await eventPublisher.PublishAsync(
                new LiteratureEvidenceDiscovered(
                    Guid.NewGuid(),
                    now,
                    statement.Id,
                    publication.Id,
                    statement.DiseaseName,
                    statement.GeneSymbol,
                    statement.RelationshipType,
                    statement.ConfidenceScore),
                publication.Id.ToString(),
                cancellationToken);
        }

        logger.LogInformation(
            "Analyzed publication {PublicationId}.",
            publication.Id);

        return Ok(publication.ToResponse());
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(
        Guid id,
        CancellationToken cancellationToken)
    {
        var publication = await dbContext.Publications
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (publication is null)
        {
            return NotFound();
        }

        dbContext.Publications.Remove(publication);

        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }
}