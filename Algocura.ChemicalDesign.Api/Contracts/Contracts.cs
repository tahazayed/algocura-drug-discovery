using Algocura.ChemicalDesign.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.ChemicalDesign.Api.Contracts;

public sealed record CreateDesignProjectRequest(
    Guid TargetAssessmentId,
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(2000)] string? Objective);

public sealed record CreateQueryStructureRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(2000)] string Structure,
    [property: Required, MaxLength(50)] string StructureFormat,
    [property: MaxLength(100)] string? SearchPurpose);

public sealed record CreateStructureSearchRequest(
    Guid QueryStructureId,
    StructureSearchType SearchType,
    decimal SimilarityThreshold = 0.70m,
    int MaximumResults = 25,
    [property: MaxLength(100)] string Provider = "Prototype");

public sealed record SelectCandidateRequest(
    [property: Required, MaxLength(500)] string SelectionRationale);

public sealed record CreateSynthesisRouteRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: MaxLength(1000)] string? Description,
    decimal EstimatedYieldPercent,
    IReadOnlyCollection<CreateReactionStepRequest> Steps);

public sealed record CreateReactionStepRequest(
    int StepNumber,
    [property: Required, MaxLength(300)] string ReactionName,
    [property: MaxLength(2000)] string? Reactants,
    [property: MaxLength(1000)] string? Reagents,
    [property: MaxLength(1000)] string? Conditions,
    [property: MaxLength(2000)] string? ProductSmiles,
    decimal EstimatedYieldPercent);

public sealed record SelectedTargetResponse(
    Guid TargetAssessmentId, Guid TargetHypothesisId, string DiseaseName,
    string GeneSymbol, string? ProteinTarget, string DesiredModulation,
    string Rationale, decimal LiteratureEvidenceScore,
    decimal TargetAssessmentScore, string ConfidenceLevel,
    DateTimeOffset SourceUpdatedAtUtc, DateTimeOffset SynchronizedAtUtc);

public sealed record QueryStructureResponse(
    Guid Id, Guid DesignProjectId, string Name, string Structure,
    string StructureFormat, string? SearchPurpose, DateTimeOffset CreatedAtUtc);

public sealed record CandidateStructureResponse(
    Guid Id, Guid DesignProjectId, Guid StructureSearchJobId, string Name,
    string CanonicalSmiles, string? InchiKey, string? MolecularFormula,
    decimal MolecularWeight, decimal MatchScore, string? SourceReference,
    string? SelectionRationale, CandidateStructureStatus Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? SelectedAtUtc);

public sealed record StructureSearchResponse(
    Guid Id, Guid DesignProjectId, Guid QueryStructureId,
    StructureSearchType SearchType, decimal SimilarityThreshold,
    int MaximumResults, string Provider, StructureSearchStatus Status,
    string? FailureReason, DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc, DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<CandidateStructureResponse> Candidates);

public sealed record ReactionStepResponse(
    Guid Id, int StepNumber, string ReactionName, string? Reactants,
    string? Reagents, string? Conditions, string? ProductSmiles,
    decimal EstimatedYieldPercent);

public sealed record SynthesisRouteResponse(
    Guid Id, Guid CandidateStructureId, string Name, string? Description,
    decimal EstimatedYieldPercent, int StepCount, SynthesisRouteStatus Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    IReadOnlyCollection<ReactionStepResponse> Steps);

public sealed record DesignProjectResponse(
    Guid Id, Guid TargetAssessmentId, string Name, string? Objective,
    string DesiredModulation, DesignProjectStatus Status,
    DateTimeOffset CreatedAtUtc, DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<QueryStructureResponse> QueryStructures,
    IReadOnlyCollection<StructureSearchResponse> SearchJobs,
    IReadOnlyCollection<CandidateStructureResponse> Candidates);
