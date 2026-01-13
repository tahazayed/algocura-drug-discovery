using System.ComponentModel.DataAnnotations;

namespace Algocura.LiteratureIntelligence.Api.Domain;

public enum PublicationStatus
{
    Imported = 1,
    PendingAnalysis = 2,
    Analyzed = 3,
    Rejected = 4,
    Archived = 5
}

public enum EvidenceStrength
{
    Low = 1,
    Moderate = 2,
    Strong = 3,
    VeryStrong = 4
}

public enum TargetHypothesisStatus
{
    Proposed = 1,
    UnderReview = 2,
    Accepted = 3,
    Rejected = 4,
    Published = 5
}

public sealed class Publication
{
    public Guid Id { get; set; }

    [MaxLength(300)]
    public required string Title { get; set; }

    [MaxLength(4000)]
    public required string Abstract { get; set; }

    [MaxLength(200)]
    public string? Journal { get; set; }

    [MaxLength(100)]
    public string? ExternalId { get; set; }

    [MaxLength(500)]
    public string? SourceUrl { get; set; }

    public DateOnly? PublishedDate { get; set; }

    public PublicationStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? AnalyzedAtUtc { get; set; }

    public List<PublicationAuthor> Authors { get; set; } = [];

    public List<EvidenceStatement> EvidenceStatements { get; set; } = [];
}

public sealed class PublicationAuthor
{
    public Guid Id { get; set; }

    public Guid PublicationId { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    public int DisplayOrder { get; set; }

    public Publication? Publication { get; set; }
}

public sealed class EvidenceStatement
{
    public Guid Id { get; set; }

    public Guid PublicationId { get; set; }

    [MaxLength(200)]
    public required string DiseaseName { get; set; }

    [MaxLength(100)]
    public required string GeneSymbol { get; set; }

    [MaxLength(200)]
    public string? ProteinTarget { get; set; }

    [MaxLength(100)]
    public required string RelationshipType { get; set; }

    [MaxLength(2000)]
    public required string Statement { get; set; }

    public EvidenceStrength Strength { get; set; }

    public decimal ConfidenceScore { get; set; }

    public bool HumanValidated { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public Publication? Publication { get; set; }
}

public sealed class TargetHypothesis
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public required string DiseaseName { get; set; }

    [MaxLength(100)]
    public required string GeneSymbol { get; set; }

    [MaxLength(200)]
    public string? ProteinTarget { get; set; }

    [MaxLength(100)]
    public required string DesiredModulation { get; set; }

    [MaxLength(2000)]
    public required string Rationale { get; set; }

    public decimal EvidenceScore { get; set; }

    public TargetHypothesisStatus Status { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<TargetHypothesisEvidence> EvidenceLinks { get; set; } = [];
}

public sealed class TargetHypothesisEvidence
{
    public Guid TargetHypothesisId { get; set; }

    public Guid EvidenceStatementId { get; set; }

    public TargetHypothesis? TargetHypothesis { get; set; }

    public EvidenceStatement? EvidenceStatement { get; set; }
}
