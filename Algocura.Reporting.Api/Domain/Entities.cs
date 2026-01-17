using Algocura.Platform.Auditing.Abstractions;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Reporting.Api.Domain;

public enum ReportDefinitionStatus { Draft = 1, Active = 2, Retired = 3 }
public enum ReportRunStatus { Queued = 1, Running = 2, Completed = 3, Failed = 4, Cancelled = 5 }
public enum ReportingProjectionType { Literature = 1, Target = 2, ChemicalDesign = 3, Compound = 4, Screening = 5, Assay = 6, Prediction = 7, Workflow = 8, Notification = 9, Search = 10 }
public enum MetricValueType { Count = 1, Percentage = 2, Decimal = 3, DurationSeconds = 4 }

public sealed class ReportDefinition : IAuditableEntity, IOwnedEntity, IApprovedEntity
{
    public Guid Id { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(2000)] public string? Description { get; set; }
    [MaxLength(100)] public required string ReportType { get; set; }
    [MaxLength(4000)] public string? DefaultParametersJson { get; set; }
    public ReportDefinitionStatus Status { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? ApprovedByUserId { get; set; }
    public DateTimeOffset? ApprovedAtUtc { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public List<ReportRun> Runs { get; set; } = [];
}

public sealed class ReportRun : IAuditableEntity, IOwnedEntity, ICompletedEntity
{
    public Guid Id { get; set; }
    public Guid ReportDefinitionId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(4000)] public string? ParametersJson { get; set; }
    public ReportRunStatus Status { get; set; }
    [MaxLength(2000)] public string? FailureReason { get; set; }
    [MaxLength(500)] public string? ResultLocation { get; set; }
    public DateTimeOffset? StartedAtUtc { get; set; }
    public Guid? StartedByUserId { get; set; }
    public Guid? CompletedByUserId { get; set; }
    public DateTimeOffset? CompletedAtUtc { get; set; }
    public Guid? OwnerUserId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ReportDefinition? ReportDefinition { get; set; }
    public List<ReportMetric> Metrics { get; set; } = [];
}

public sealed class ReportMetric : IAuditableEntity
{
    public Guid Id { get; set; }
    public Guid ReportRunId { get; set; }
    [MaxLength(200)] public required string Name { get; set; }
    [MaxLength(500)] public string? Description { get; set; }
    public decimal Value { get; set; }
    public MetricValueType ValueType { get; set; }
    [MaxLength(100)] public string? Unit { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
    public ReportRun? ReportRun { get; set; }
}

public sealed class ReportingProjection : IAuditableEntity, IProjectionAuditMetadata
{
    public Guid Id { get; set; }
    public Guid SourceEntityId { get; set; }
    public ReportingProjectionType ProjectionType { get; set; }
    [MaxLength(200)] public required string SourceEventType { get; set; }
    [MaxLength(200)] public required string SourceService { get; set; }
    [MaxLength(300)] public required string Title { get; set; }
    [MaxLength(4000)] public string? Summary { get; set; }
    [MaxLength(200)] public string? DiseaseName { get; set; }
    [MaxLength(100)] public string? GeneSymbol { get; set; }
    [MaxLength(200)] public string? CompoundName { get; set; }
    [MaxLength(100)] public string? Outcome { get; set; }
    public decimal? Score { get; set; }
    [MaxLength(100)] public string? WorkflowId { get; set; }
    [MaxLength(100)] public string? CorrelationId { get; set; }
    public DateTimeOffset SourceUpdatedAtUtc { get; set; }
    public DateTimeOffset SynchronizedAtUtc { get; set; }
    public Guid? SourceCreatedByUserId { get; set; }
    public Guid? SourceUpdatedByUserId { get; set; }
    public Guid? CreatedByUserId { get; set; }
    public Guid? UpdatedByUserId { get; set; }
    public DateTimeOffset CreatedAtUtc { get; set; }
    public DateTimeOffset UpdatedAtUtc { get; set; }
}
