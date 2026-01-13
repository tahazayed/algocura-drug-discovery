using Algocura.LiteratureIntelligence.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.LiteratureIntelligence.Api.Contracts;

public sealed record CreatePublicationRequest(
    [property: Required, MaxLength(300)] string Title,
    [property: Required, MaxLength(4000)] string Abstract,
    [property: MaxLength(200)] string? Journal,
    [property: MaxLength(100)] string? ExternalId,
    [property: MaxLength(500)] string? SourceUrl,
    DateOnly? PublishedDate,
    IReadOnlyCollection<string>? Authors);

public sealed record AnalyzePublicationRequest(
    [property: Required, MaxLength(200)] string DiseaseName,
    IReadOnlyCollection<string>? CandidateGeneSymbols);

public sealed record CreateTargetHypothesisRequest(
    [property: Required, MaxLength(200)] string DiseaseName,
    [property: Required, MaxLength(100)] string GeneSymbol,
    [property: MaxLength(200)] string? ProteinTarget,
    [property: Required, MaxLength(100)] string DesiredModulation,
    [property: Required, MaxLength(2000)] string Rationale,
    IReadOnlyCollection<Guid> EvidenceStatementIds);

public sealed record EvidenceStatementResponse(
    Guid Id,
    Guid PublicationId,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string RelationshipType,
    string Statement,
    EvidenceStrength Strength,
    decimal ConfidenceScore,
    bool HumanValidated,
    DateTimeOffset CreatedAtUtc);

public sealed record PublicationResponse(
    Guid Id,
    string Title,
    string Abstract,
    string? Journal,
    string? ExternalId,
    string? SourceUrl,
    DateOnly? PublishedDate,
    PublicationStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? AnalyzedAtUtc,
    IReadOnlyCollection<string> Authors,
    IReadOnlyCollection<EvidenceStatementResponse> EvidenceStatements);

public sealed record TargetHypothesisResponse(
    Guid Id,
    string DiseaseName,
    string GeneSymbol,
    string? ProteinTarget,
    string DesiredModulation,
    string Rationale,
    decimal EvidenceScore,
    TargetHypothesisStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<Guid> EvidenceStatementIds);
