using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Domain;
using Algocura.LiteratureIntelligence.Api.Events;
using Algocura.LiteratureIntelligence.Api.Mapping;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/target-hypotheses")]
public sealed class TargetHypothesesController(
    LiteratureIntelligenceDbContext dbContext,
    IEventPublisher eventPublisher,
    ILogger<TargetHypothesesController> logger)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<TargetHypothesisResponse>>> GetAll(
            TargetHypothesisStatus? status,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        var query = dbContext.TargetHypotheses
            .AsNoTracking()
            .Include(x => x.EvidenceLinks)
            .AsQueryable();

        if (status.HasValue)
        {
            query = query.Where(x => x.Status == status.Value);
        }

        var hypotheses = await query
            .OrderByDescending(x => x.EvidenceScore)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(
            hypotheses
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<TargetHypothesisResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var hypothesis = await dbContext.TargetHypotheses
            .AsNoTracking()
            .Include(x => x.EvidenceLinks)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        return hypothesis is null
            ? NotFound()
            : Ok(hypothesis.ToResponse());
    }

    [HttpPost]
    public async Task<ActionResult<TargetHypothesisResponse>> Create(
        CreateTargetHypothesisRequest request,
        CancellationToken cancellationToken)
    {
        var evidenceIds = request.EvidenceStatementIds
            .Distinct()
            .ToArray();

        if (evidenceIds.Length == 0)
        {
            ModelState.AddModelError(
                nameof(request.EvidenceStatementIds),
                "At least one evidence statement is required.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        var evidence = await dbContext.EvidenceStatements
            .Where(x => evidenceIds.Contains(x.Id))
            .ToListAsync(cancellationToken);

        if (evidence.Count != evidenceIds.Length)
        {
            ModelState.AddModelError(
                nameof(request.EvidenceStatementIds),
                "One or more evidence statement IDs are unknown.");

            return BadRequest(
                new ValidationProblemDetails(ModelState));
        }

        var now = DateTimeOffset.UtcNow;

        var hypothesis = new TargetHypothesis
        {
            Id = Guid.NewGuid(),
            DiseaseName = request.DiseaseName.Trim(),
            GeneSymbol = request.GeneSymbol.Trim().ToUpperInvariant(),
            ProteinTarget = request.ProteinTarget?.Trim(),
            DesiredModulation = request.DesiredModulation.Trim(),
            Rationale = request.Rationale.Trim(),
            EvidenceScore = decimal.Round(
                evidence.Average(x => x.ConfidenceScore),
                4),
            Status = TargetHypothesisStatus.Proposed,
            CreatedAtUtc = now,
            UpdatedAtUtc = now
        };

        foreach (var statement in evidence)
        {
            hypothesis.EvidenceLinks.Add(
                new TargetHypothesisEvidence
                {
                    TargetHypothesisId = hypothesis.Id,
                    EvidenceStatementId = statement.Id
                });
        }

        dbContext.TargetHypotheses.Add(hypothesis);
        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new TargetHypothesisProposed(
                Guid.NewGuid(),
                now,
                hypothesis.Id,
                hypothesis.DiseaseName,
                hypothesis.GeneSymbol,
                hypothesis.ProteinTarget,
                hypothesis.DesiredModulation,
                hypothesis.EvidenceScore),
            correlationId: hypothesis.Id.ToString(),
            cancellationToken: cancellationToken);

        logger.LogInformation(
            "Proposed target hypothesis {TargetHypothesisId}.",
            hypothesis.Id);

        return CreatedAtAction(
            nameof(GetById),
            new { id = hypothesis.Id },
            hypothesis.ToResponse());
    }

    [HttpPost("{id:guid}/publish")]
    public async Task<ActionResult<TargetHypothesisResponse>> Publish(
        Guid id,
        CancellationToken cancellationToken)
    {
        var hypothesis = await dbContext.TargetHypotheses
            .Include(x => x.EvidenceLinks)
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (hypothesis is null)
        {
            return NotFound();
        }

        hypothesis.Status = TargetHypothesisStatus.Published;
        hypothesis.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        await eventPublisher.PublishAsync(
            new LiteratureIntelligenceSnapshot(
                Guid.NewGuid(),
                hypothesis.UpdatedAtUtc,
                hypothesis.Id,
                hypothesis.DiseaseName,
                hypothesis.GeneSymbol,
                hypothesis.ProteinTarget,
                hypothesis.DesiredModulation,
                hypothesis.Rationale,
                hypothesis.EvidenceScore,
                hypothesis.EvidenceLinks
                    .Select(x => x.EvidenceStatementId)
                    .ToArray()),
            correlationId: hypothesis.Id.ToString(),
            cancellationToken: cancellationToken);

        return Ok(hypothesis.ToResponse());
    }
}
