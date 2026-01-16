using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.TargetDiscovery.Api.Domain;

public sealed class TargetHypothesisProjection
    : IAuditableEntity
{
    public Guid TargetHypothesisId { get; set; }

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

    public decimal LiteratureEvidenceScore { get; set; }

    public TargetHypothesisProjectionStatus Status { get; set; }

    public DateTimeOffset SourceUpdatedAtUtc { get; set; }

    public DateTimeOffset SynchronizedAtUtc { get; set; }

    public List<TargetHypothesisEvidenceProjection> EvidenceLinks { get; set; } = [];

    public List<TargetAssessment> Assessments { get; set; } = [];

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}

public sealed class TargetHypothesisEvidenceProjection
    : IAuditableEntity
{
    public Guid TargetHypothesisId { get; set; }

    public Guid EvidenceStatementId { get; set; }

    public TargetHypothesisProjection? TargetHypothesis { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
