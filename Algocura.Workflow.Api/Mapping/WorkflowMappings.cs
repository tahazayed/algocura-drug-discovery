using Algocura.Workflow.Api.Contracts;
using Algocura.Workflow.Api.Domain;

namespace Algocura.Workflow.Api.Mapping;

public static class WorkflowMappings
{
    public static DiscoveryWorkflowResponse ToResponse(
        this DiscoveryWorkflow workflow)
    {
        return new DiscoveryWorkflowResponse(
            workflow.Id,
            workflow.Name,
            workflow.DiseaseName,
            workflow.GeneSymbol,
            workflow.Objective,
            workflow.Status,
            workflow.CurrentStage,
            workflow.CreatedAtUtc,
            workflow.UpdatedAtUtc,
            workflow.StartedAtUtc,
            workflow.CompletedAtUtc,
            workflow.Stages
                .OrderBy(x => x.Stage)
                .Select(x => new WorkflowStageResponse(
                    x.Id,
                    x.Stage,
                    x.Status,
                    x.ExternalEntityId,
                    x.ExternalEntityType,
                    x.Summary,
                    x.FailureReason,
                    x.StartedAtUtc,
                    x.CompletedAtUtc,
                    x.UpdatedAtUtc))
                .ToArray());
    }
}
