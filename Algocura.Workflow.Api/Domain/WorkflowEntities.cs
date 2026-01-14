using System.ComponentModel.DataAnnotations;

namespace Algocura.Workflow.Api.Domain;

public enum DiscoveryWorkflowStatus
{
    Draft = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Cancelled = 5
}

public enum WorkflowStage
{
    LiteratureIntelligence = 1,
    TargetDiscovery = 2,
    ChemicalDesign = 3,
    CompoundCatalog = 4,
    Screening = 5,
    AssayManagement = 6,
    AiDiscovery = 7
}

public enum WorkflowStageStatus
{
    Pending = 1,
    InProgress = 2,
    Completed = 3,
    Failed = 4,
    Skipped = 5
}

public sealed class DiscoveryWorkflow
{
    public Guid Id { get; set; }

    [MaxLength(200)]
    public required string Name { get; set; }

    [MaxLength(200)]
    public required string DiseaseName { get; set; }

    [MaxLength(100)]
    public string? GeneSymbol { get; set; }

    [MaxLength(2000)]
    public string? Objective { get; set; }

    public DiscoveryWorkflowStatus Status { get; set; }

    public WorkflowStage CurrentStage { get; set; }

    public DateTimeOffset CreatedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public List<WorkflowStageExecution> Stages { get; set; } = [];

    public List<WorkflowEventReceipt> EventReceipts { get; set; } = [];
}

public sealed class WorkflowStageExecution
{
    public Guid Id { get; set; }

    public Guid WorkflowId { get; set; }

    public WorkflowStage Stage { get; set; }

    public WorkflowStageStatus Status { get; set; }

    public Guid? ExternalEntityId { get; set; }

    [MaxLength(200)]
    public string? ExternalEntityType { get; set; }

    [MaxLength(2000)]
    public string? Summary { get; set; }

    [MaxLength(2000)]
    public string? FailureReason { get; set; }

    public DateTimeOffset? StartedAtUtc { get; set; }

    public DateTimeOffset? CompletedAtUtc { get; set; }

    public DateTimeOffset UpdatedAtUtc { get; set; }

    public DiscoveryWorkflow? Workflow { get; set; }
}

public sealed class WorkflowEventReceipt
{
    public Guid EventId { get; set; }

    public Guid WorkflowId { get; set; }

    [MaxLength(200)]
    public required string EventType { get; set; }

    [MaxLength(200)]
    public required string Source { get; set; }

    public DateTimeOffset OccurredAtUtc { get; set; }

    public DateTimeOffset ProcessedAtUtc { get; set; }

    public DiscoveryWorkflow? Workflow { get; set; }
}
