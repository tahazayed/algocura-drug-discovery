using System.ComponentModel.DataAnnotations;

namespace Algocura.AIDiscovery.Api.Contracts;

public sealed record AdaptiveRunPredictionRequest(
    [property: Range(typeof(decimal), "0.01", "1.00")] decimal ResourceBudget = 1.00m);

public sealed record AdaptiveModelExecutionResponse(
    Guid ModelDefinitionId, string ModelName, decimal Confidence,
    decimal ResourceCost, decimal RoutingScore, string RoutingReason);

public sealed record AdaptiveRunPredictionResponse(
    PredictionResponse Prediction, decimal FinalConfidence,
    decimal ResourceConsumed,
    IReadOnlyCollection<AdaptiveModelExecutionResponse> Executions);
