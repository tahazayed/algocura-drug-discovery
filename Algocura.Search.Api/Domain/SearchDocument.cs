using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Search.Api.Domain;

public enum SearchDocumentType
{
    Publication = 1,
    Target = 2,
    ChemicalDesign = 3,
    Compound = 4,
    Screening = 5,
    Assay = 6,
    Prediction = 7,
    Workflow = 8,
    Notification = 9
}

public enum SearchDocumentStatus
{
    Active = 1,
    Archived = 2,
    Deleted = 3
}

public sealed class SearchDocument
    : IAuditableEntity,
      IProjectionAuditMetadata
{
    public Guid Id { get; set; }
    public Guid SourceEntityId { get; set; }
    [MaxLength(200)] public required string SourceEventType { get; set; }
    [MaxLength(200)] public required string SourceService { get; set; }
    public SearchDocumentType DocumentType { get; set; }
    [MaxLength(300)] public required string Title { get; set; }
    [MaxLength(4000)] public string? Summary { get; set; }
    [MaxLength(100)] public string? ExternalReference { get; set; }
    [MaxLength(100)] public string? CorrelationId { get; set; }
    [MaxLength(100)] public string? WorkflowId { get; set; }
    [MaxLength(200)] public string? DiseaseName { get; set; }
    [MaxLength(100)] public string? GeneSymbol { get; set; }
    [MaxLength(200)] public string? CompoundName { get; set; }
    [MaxLength(27)] public string? InchiKey { get; set; }
    [MaxLength(2000)] public string? CanonicalSmiles { get; set; }
    [MaxLength(100)] public string? Outcome { get; set; }
    public decimal? Score { get; set; }
    public SearchDocumentStatus Status { get; set; }
    public DateTimeOffset SourceUpdatedAtUtc { get; set; }
    public DateTimeOffset IndexedAtUtc { get; set; }

    public Guid? SourceCreatedByUserId { get; set; }

    public Guid? SourceUpdatedByUserId { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public List<SearchDocumentTag> Tags { get; set; } = [];
}

public sealed class SearchDocumentTag
    : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid SearchDocumentId { get; set; }
    [MaxLength(100)] public required string Name { get; set; }
    [MaxLength(500)] public required string Value { get; set; }

    public SearchDocument? SearchDocument { get; set; }

    public Guid? CreatedByUserId { get; set; }

    public Guid? UpdatedByUserId { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }
}
