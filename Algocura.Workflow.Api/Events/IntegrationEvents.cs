namespace Algocura.Workflow.Api.Events;

public abstract record IntegrationEvent(
    Guid EventId,
    DateTimeOffset OccurredAtUtc)
    : Algocura.Platform.Messaging.Events.IntegrationEvent(
        EventId,
        OccurredAtUtc)
{
    public override string Source =>
        "Algocura.Workflow.Api";
}

public sealed record WorkflowCreated(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid WorkflowId,
    string Name,
    string DiseaseName,
    string? GeneSymbol)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(WorkflowCreated);
}

public sealed record WorkflowStageCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid WorkflowId,
    string Stage,
    Guid? ExternalEntityId,
    string? ExternalEntityType)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(WorkflowStageCompleted);
}

public sealed record WorkflowCompleted(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid WorkflowId,
    string Name)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(WorkflowCompleted);
}

public sealed record WorkflowSnapshot(
    Guid EventId,
    DateTimeOffset OccurredAtUtc,
    Guid WorkflowId,
    string Name,
    string DiseaseName,
    string? GeneSymbol,
    string Status,
    string CurrentStage,
    IReadOnlyCollection<WorkflowStageSnapshot> Stages)
    : IntegrationEvent(EventId, OccurredAtUtc)
{
    public override string EventType =>
        nameof(WorkflowSnapshot);
}

public sealed record WorkflowStageSnapshot(
    string Stage,
    string Status,
    Guid? ExternalEntityId,
    string? ExternalEntityType,
    string? Summary);
