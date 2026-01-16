using Algocura.Search.Api.Contracts;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace Algocura.Search.Api.Controllers;

[ApiController]
[Route("api/v1/search")]
public sealed class SearchController(
    ISearchService searchService)
    : ControllerBase
{
    [HttpGet]
    public Task<SearchResponse> Search(
        [FromQuery] string? query,
        [FromQuery] SearchDocumentType? documentType,
        [FromQuery] string? diseaseName,
        [FromQuery] string? geneSymbol,
        [FromQuery] string? compoundName,
        [FromQuery] string? inchiKey,
        [FromQuery] string? workflowId,
        [FromQuery] SearchDocumentStatus? status,
        [FromQuery] decimal? minimumScore,
        [FromQuery] int skip = 0,
        [FromQuery] int take = 50,
        CancellationToken cancellationToken = default)
    {
        return searchService.SearchAsync(
            new SearchRequest(
                query,
                documentType,
                diseaseName,
                geneSymbol,
                compoundName,
                inchiKey,
                workflowId,
                status,
                minimumScore,
                skip,
                take),
            cancellationToken);
    }
}

