using Algocura.Workflow.Api.Domain;
using System.ComponentModel.DataAnnotations;

namespace Algocura.Workflow.Api.Contracts;

public sealed record CreateWorkflowRequest(
    [property: Required, MaxLength(200)] string Name,
    [property: Required, MaxLength(200)] string DiseaseName,
    [property: MaxLength(100)] string? GeneSymbol,
    [property: MaxLength(2000)] string? Objective);

public sealed record FailWorkflowStageRequest(
    [property: Required, MaxLength(2000)] string FailureReason);

public sealed record WorkflowStageResponse(
    Guid Id,
    WorkflowStage Stage,
    WorkflowStageStatus Status,
    Guid? ExternalEntityId,
    string? ExternalEntityType,
    string? Summary,
    string? FailureReason,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    DateTimeOffset UpdatedAtUtc);

public sealed record DiscoveryWorkflowResponse(
    Guid Id,
    string Name,
    string DiseaseName,
    string? GeneSymbol,
    string? Objective,
    DiscoveryWorkflowStatus Status,
    WorkflowStage CurrentStage,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    IReadOnlyCollection<WorkflowStageResponse> Stages);
