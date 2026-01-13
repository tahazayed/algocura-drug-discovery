using Algocura.TargetDiscovery.Api.Contracts;
using Algocura.TargetDiscovery.Api.Data;
using Algocura.TargetDiscovery.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.TargetDiscovery.Api.Controllers;

[ApiController]
[Route("api/v1/target-hypotheses")]
public sealed class TargetHypothesesController(
    TargetDiscoveryDbContext dbContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<TargetHypothesisResponse>>> GetAll(
            string? disease,
            string? gene,
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

        if (!string.IsNullOrWhiteSpace(disease))
        {
            var value = disease.Trim();

            query = query.Where(
                x => x.DiseaseName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(gene))
        {
            var value =
                gene.Trim().ToUpperInvariant();

            query = query.Where(
                x => x.GeneSymbol == value);
        }

        var hypotheses = await query
            .OrderByDescending(x => x.LiteratureEvidenceScore)
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
                x => x.TargetHypothesisId == id,
                cancellationToken);

        return hypothesis is null
            ? NotFound()
            : Ok(hypothesis.ToResponse());
    }
}
