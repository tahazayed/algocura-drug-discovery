using Algocura.Search.Api.Contracts;
using Algocura.Search.Api.Data;
using Algocura.Search.Api.Domain;
using Algocura.Search.Api.Mapping;
using Microsoft.EntityFrameworkCore;

namespace Algocura.Search.Api.Services;

public interface ISearchService
{
    Task<SearchResponse> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default);
}

public sealed class SearchService(
    SearchDbContext dbContext)
    : ISearchService
{
    public async Task<SearchResponse> SearchAsync(
        SearchRequest request,
        CancellationToken cancellationToken = default)
    {
        var skip = Math.Max(request.Skip, 0);
        var take = Math.Clamp(request.Take, 1, 200);

        var query =
            dbContext.SearchDocuments
                .AsNoTracking()
                .Include(x => x.Tags)
                .AsQueryable();

        if (request.DocumentType.HasValue)
        {
            query = query.Where(
                x => x.DocumentType == request.DocumentType.Value);
        }

        if (request.Status.HasValue)
        {
            query = query.Where(
                x => x.Status == request.Status.Value);
        }
        else
        {
            query = query.Where(
                x => x.Status == SearchDocumentStatus.Active);
        }

        if (!string.IsNullOrWhiteSpace(request.DiseaseName))
        {
            var value = request.DiseaseName.Trim();

            query = query.Where(
                x =>
                    x.DiseaseName != null &&
                    x.DiseaseName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(request.GeneSymbol))
        {
            var value =
                request.GeneSymbol.Trim().ToUpperInvariant();

            query = query.Where(
                x => x.GeneSymbol == value);
        }

        if (!string.IsNullOrWhiteSpace(request.CompoundName))
        {
            var value = request.CompoundName.Trim();

            query = query.Where(
                x =>
                    x.CompoundName != null &&
                    x.CompoundName.Contains(value));
        }

        if (!string.IsNullOrWhiteSpace(request.InchiKey))
        {
            var value =
                request.InchiKey.Trim().ToUpperInvariant();

            query = query.Where(
                x => x.InchiKey == value);
        }

        if (!string.IsNullOrWhiteSpace(request.WorkflowId))
        {
            var value = request.WorkflowId.Trim();

            query = query.Where(
                x => x.WorkflowId == value);
        }

        if (request.MinimumScore.HasValue)
        {
            query = query.Where(
                x =>
                    x.Score != null &&
                    x.Score >= request.MinimumScore.Value);
        }

        if (!string.IsNullOrWhiteSpace(request.Query))
        {
            var value = request.Query.Trim();

            query = query.Where(
                x =>
                    x.Title.Contains(value) ||
                    (x.Summary != null &&
                     x.Summary.Contains(value)) ||
                    (x.DiseaseName != null &&
                     x.DiseaseName.Contains(value)) ||
                    (x.GeneSymbol != null &&
                     x.GeneSymbol.Contains(value)) ||
                    (x.CompoundName != null &&
                     x.CompoundName.Contains(value)) ||
                    (x.InchiKey != null &&
                     x.InchiKey.Contains(value)) ||
                    x.Tags.Any(
                        tag =>
                            tag.Name.Contains(value) ||
                            tag.Value.Contains(value)));
        }

        var total =
            await query.CountAsync(cancellationToken);

        // DateTimeOffset ordering is performed in memory so SQLite tests work.
        var items =
            (await query.ToListAsync(cancellationToken))
            .OrderByDescending(x => x.Score ?? 0m)
            .ThenByDescending(x => x.UpdatedAtUtc)
            .ThenBy(x => x.Title)
            .Skip(skip)
            .Take(take)
            .Select(x => x.ToResponse())
            .ToArray();

        return new SearchResponse(
            total,
            skip,
            take,
            items);
    }
}

