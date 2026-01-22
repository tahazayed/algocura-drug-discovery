using Algocura.AIDiscovery.Api.Domain;

namespace Algocura.AIDiscovery.Api.Routing;

public sealed record PreviousModelExecution(Guid ModelDefinitionId, decimal Confidence, decimal ResourceCost);

public sealed record ModelRoutingContext(
    IReadOnlyCollection<PredictionCandidate> Candidates,
    IReadOnlyCollection<ModelDefinition> AvailableModels,
    IReadOnlyCollection<PreviousModelExecution> PreviousExecutions,
    decimal RemainingResourceBudget);

public sealed record ModelRoutingCandidateScore(
    Guid ModelDefinitionId, string ModelName, decimal MolecularSuitability,
    decimal ExpectedConfidence, decimal Specialization,
    decimal NormalizedResourceCost, decimal RoutingScore);

public sealed record ModelRoutingResult(
    Guid SelectedModelDefinitionId, string SelectedModelName,
    decimal RoutingScore, string Reason,
    IReadOnlyCollection<ModelRoutingCandidateScore> Candidates);
