using Algocura.LiteratureIntelligence.Api.Contracts;
using Algocura.LiteratureIntelligence.Api.Data;
using Algocura.LiteratureIntelligence.Api.Mapping;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.LiteratureIntelligence.Api.Controllers;

[ApiController]
[Route("api/v1/evidence")]
public sealed class EvidenceController(
    LiteratureIntelligenceDbContext dbContext)
    : ControllerBase
{
    [HttpGet]
    public async Task<
        ActionResult<IReadOnlyCollection<EvidenceStatementResponse>>> GetAll(
            string? disease,
            string? gene,
            bool? humanValidated,
            int skip = 0,
            int take = 50,
            CancellationToken cancellationToken = default)
    {
        skip = Math.Max(skip, 0);
        take = Math.Clamp(take, 1, 200);

        var query = dbContext.EvidenceStatements
            .AsNoTracking()
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(disease))
        {
            query = query.Where(
                x => x.DiseaseName.Contains(disease.Trim()));
        }

        if (!string.IsNullOrWhiteSpace(gene))
        {
            query = query.Where(
                x => x.GeneSymbol == gene.Trim().ToUpperInvariant());
        }

        if (humanValidated.HasValue)
        {
            query = query.Where(
                x => x.HumanValidated == humanValidated.Value);
        }

        var statements = await query
            .OrderByDescending(x => x.ConfidenceScore)
            .ThenByDescending(x => x.CreatedAtUtc)
            .Skip(skip)
            .Take(take)
            .ToListAsync(cancellationToken);

        return Ok(
            statements
                .Select(x => x.ToResponse())
                .ToArray());
    }

    [HttpPost("{id:guid}/validate")]
    public async Task<ActionResult<EvidenceStatementResponse>> Validate(
        Guid id,
        CancellationToken cancellationToken)
    {
        var statement = await dbContext.EvidenceStatements
            .SingleOrDefaultAsync(
                x => x.Id == id,
                cancellationToken);

        if (statement is null)
        {
            return NotFound();
        }

        statement.HumanValidated = true;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(statement.ToResponse());
    }
}
