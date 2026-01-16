using Algocura.Search.Api.Domain;

namespace Algocura.Search.Api.Contracts;

public sealed record SearchRequest(
    string? Query,
    SearchDocumentType? DocumentType,
    string? DiseaseName,
    string? GeneSymbol,
    string? CompoundName,
    string? InchiKey,
    string? WorkflowId,
    SearchDocumentStatus? Status,
    decimal? MinimumScore,
    int Skip = 0,
    int Take = 50);

public sealed record SearchTagResponse(
    string Name,
    string Value);

public sealed record SearchDocumentResponse(
    Guid Id,
    Guid SourceEntityId,
    string SourceEventType,
    string SourceService,
    SearchDocumentType DocumentType,
    string Title,
    string? Summary,
    string? ExternalReference,
    string? CorrelationId,
    string? WorkflowId,
    string? DiseaseName,
    string? GeneSymbol,
    string? CompoundName,
    string? InchiKey,
    string? CanonicalSmiles,
    string? Outcome,
    decimal? Score,
    SearchDocumentStatus Status,
    DateTimeOffset SourceUpdatedAtUtc,
    DateTimeOffset IndexedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<SearchTagResponse> Tags);

public sealed record SearchResponse(
    int Total,
    int Skip,
    int Take,
    IReadOnlyCollection<SearchDocumentResponse> Items);

