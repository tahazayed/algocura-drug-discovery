using Algocura.Search.Api.Contracts;
using Algocura.Search.Api.Data;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Mapping;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Controllers;

[Authorize(Roles = "Scientist,Reviewer,Administrator")]
[ApiController]
[Route("api/v1/search-documents")]
public sealed class SearchDocumentsController(
    SearchDbContext dbContext)
    : ControllerBase
{
    [HttpGet("{id:guid}")]
    public async Task<ActionResult<SearchDocumentResponse>> GetById(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document =
            await dbContext.SearchDocuments
                .AsNoTracking()
                .Include(x => x.Tags)
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        return document is null
            ? NotFound()
            : Ok(document.ToResponse());
    }

    [HttpPost("{id:guid}/archive")]
    public async Task<ActionResult<SearchDocumentResponse>> Archive(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document =
            await dbContext.SearchDocuments
                .Include(x => x.Tags)
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        document.Status = SearchDocumentStatus.Archived;
        document.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(document.ToResponse());
    }

    [HttpPost("{id:guid}/restore")]
    public async Task<ActionResult<SearchDocumentResponse>> Restore(
        Guid id,
        CancellationToken cancellationToken)
    {
        var document =
            await dbContext.SearchDocuments
                .Include(x => x.Tags)
                .SingleOrDefaultAsync(
                    x => x.Id == id,
                    cancellationToken);

        if (document is null)
        {
            return NotFound();
        }

        document.Status = SearchDocumentStatus.Active;
        document.UpdatedAtUtc = DateTimeOffset.UtcNow;

        await dbContext.SaveChangesAsync(cancellationToken);

        return Ok(document.ToResponse());
    }
}

