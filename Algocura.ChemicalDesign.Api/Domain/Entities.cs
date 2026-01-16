using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.ChemicalDesign.Api.Domain;

public enum DesignProjectStatus { Draft = 1, InProgress = 2, Completed = 3, Cancelled = 4 }
public enum StructureSearchType { Substructure = 1, Similarity = 2, Exact = 3 }
public enum StructureSearchStatus { Queued = 1, Running = 2, Completed = 3, Failed = 4 }
public enum CandidateStructureStatus { Proposed = 1, UnderReview = 2, SelectedForRegistration = 3, Rejected = 4, Registered = 5 }
public enum SynthesisRouteStatus { Draft = 1, Feasible = 2, Preferred = 3, Rejected = 4 }

public sealed class SelectedTargetProjection
    : IAuditableEntity,
      IProjectionAuditMetadata
{
    public Guid TargetAssessmentId { get; set; }
    public Guid TargetHypothesisId { get; set; }
    [MaxLength(200)] public required string DiseaseName { get; set; }
    [MaxLength(100)] public required string GeneSymbol { get; set; }
    [MaxLength(200)] public string? ProteinTarget { get; set; }
    [MaxLength(100)] public required string DesiredModulation { get; set; }
    [MaxLength(2000)] public required string Rationale { get; set; }
    public decimal LiteratureEvidenceScore { get; set; }
    public decimal TargetAssessmentScore { get; set; }
    [MaxLength(50)] public required string ConfidenceLevel { get; set; }
    public DateTimeOffset SourceUpdatedAtUtc { get; set; }
    public DateTimeOffset SynchronizedAtUtc { get; set; }
    public List<ChemicalDesignProject> DesignProjects { get; set; } = [];
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
    public Guid? SourceCreatedByUserId { get; set; }
    public Guid? SourceUpdatedByUserId { get; set; }
}

public sealed class ChemicalDesignProject
    : IAuditableEntity,
      IOwnedEntity,
      ICompletedEntity
{
    public Guid Id { get; set; }
    public Guid TargetAssessmentId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(2000)] public string? Objective { get; set; }
    [MaxLength(100)] public required string DesiredModulation { get; set; }
    public DesignProjectStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public SelectedTargetProjection? SelectedTarget { get; set; }
    public List<QueryStructure> QueryStructures { get; set; } = [];
    public List<StructureSearchJob> SearchJobs { get; set; } = [];
    public List<CandidateStructure> Candidates { get; set; } = [];
    public Guid? OwnerUserId { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class QueryStructure
    : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid DesignProjectId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(2000)] public required string Structure { get; set; }
    [MaxLength(50)] public required string StructureFormat { get; set; }
    [MaxLength(100)] public string? SearchPurpose { get; set; }

    public ChemicalDesignProject? DesignProject { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class StructureSearchJob
    : IAuditableEntity,
      ICompletedEntity
{
    public Guid Id { get; set; }
    public Guid DesignProjectId { get; set; }
    public Guid QueryStructureId { get; set; }
    public StructureSearchType SearchType { get; set; }
    public decimal SimilarityThreshold { get; set; }
    public int MaximumResults { get; set; }
    [MaxLength(100)] public required string Provider { get; set; }
    public StructureSearchStatus Status { get; set; }
    [MaxLength(2000)] public string? FailureReason { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public ChemicalDesignProject? DesignProject { get; set; }
    public QueryStructure? QueryStructure { get; set; }
    public List<CandidateStructure> Candidates { get; set; } = [];
    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class CandidateStructure
    : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid DesignProjectId { get; set; }
    public Guid StructureSearchJobId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(2000)] public required string CanonicalSmiles { get; set; }
    [MaxLength(27)] public string? InchiKey { get; set; }
    [MaxLength(100)] public string? MolecularFormula { get; set; }
    public decimal MolecularWeight { get; set; }
    public decimal MatchScore { get; set; }
    [MaxLength(100)] public string? SourceReference { get; set; }
    [MaxLength(500)] public string? SelectionRationale { get; set; }
    public CandidateStructureStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? SelectedByUserId { get; set; }
    public DateTimeOffset? SelectedAtUtc { get; set; }
    public ChemicalDesignProject? DesignProject { get; set; }
    public StructureSearchJob? StructureSearchJob { get; set; }
    public List<SynthesisRoute> SynthesisRoutes { get; set; } = [];

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class SynthesisRoute
    : IAuditableEntity,
      IApprovedEntity
{
    public Guid Id { get; set; }
    public Guid CandidateStructureId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(1000)] public string? Description { get; set; }
    public decimal EstimatedYieldPercent { get; set; }
    public int StepCount { get; set; }
    public SynthesisRouteStatus Status { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public CandidateStructure? CandidateStructure { get; set; }
    public List<ReactionStep> Steps { get; set; } = [];

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class ReactionStep
    : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid SynthesisRouteId { get; set; }
    public int StepNumber { get; set; }
    [MaxLength(300)] public required string ReactionName { get; set; }
    [MaxLength(2000)] public string? Reactants { get; set; }
    [MaxLength(1000)] public string? Reagents { get; set; }
    [MaxLength(1000)] public string? Conditions { get; set; }
    [MaxLength(2000)] public string? ProductSmiles { get; set; }
    public decimal EstimatedYieldPercent { get; set; }
    public SynthesisRoute? SynthesisRoute { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}

public sealed class ChemicalLibraryEntry
    : IAuditableEntity
{
    public Guid Id { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(2000)] public required string CanonicalSmiles { get; set; }
    [MaxLength(27)] public string? InchiKey { get; set; }
    [MaxLength(100)] public string? MolecularFormula { get; set; }
    public decimal MolecularWeight { get; set; }
    [MaxLength(100)] public required string SourceReference { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }
}
