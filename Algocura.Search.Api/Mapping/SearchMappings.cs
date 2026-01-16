using Algocura.Search.Api.Contracts;
using Algocura.Search.Api.Domain;

namespace Algocura.Search.Api.Mapping;

public static class SearchMappings
{
    public static SearchDocumentResponse ToResponse(
        this SearchDocument document)
    {
        return new SearchDocumentResponse(
            document.Id,
            document.SourceEntityId,
            document.SourceEventType,
            document.SourceService,
            document.DocumentType,
            document.Title,
            document.Summary,
            document.ExternalReference,
            document.CorrelationId,
            document.WorkflowId,
            document.DiseaseName,
            document.GeneSymbol,
            document.CompoundName,
            document.InchiKey,
            document.CanonicalSmiles,
            document.Outcome,
            document.Score,
            document.Status,
            document.SourceUpdatedAtUtc,
            document.IndexedAtUtc,
            document.UpdatedAtUtc,
            document.Tags
                .OrderBy(x => x.Name)
                .ThenBy(x => x.Value)
                .Select(x => new SearchTagResponse(
                    x.Name,
                    x.Value))
                .ToArray());
    }
}

