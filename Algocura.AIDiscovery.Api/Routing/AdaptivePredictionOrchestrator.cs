using Algocura.AIDiscovery.Api.Domain;
using Algocura.AIDiscovery.Api.Services;
using Microsoft.Extensions.Options;

namespace Algocura.AIDiscovery.Api.Routing;

public interface IAdaptivePredictionOrchestrator
{
    AdaptivePredictionExecution Execute(PredictionJob job, IReadOnlyCollection<ModelDefinition> availableModels, decimal resourceBudget);
}

public sealed record AdaptiveModelExecution(
    Guid ModelDefinitionId, string ModelName, decimal Confidence,
    decimal ResourceCost, decimal RoutingScore, string RoutingReason);

public sealed record AdaptivePredictionExecution(
    ModelDefinition FinalModel, IReadOnlyCollection<PredictionResult> Results,
    decimal FinalConfidence, decimal ResourceConsumed,
    IReadOnlyCollection<AdaptiveModelExecution> Executions);

public sealed class AdaptivePredictionOrchestrator(
    IAdaptiveModelRouter router,
    IPredictionScoringEngine scoringEngine,
    IOptions<ModelRoutingOptions> options) : IAdaptivePredictionOrchestrator
{
    public AdaptivePredictionExecution Execute(
        PredictionJob job,
        IReadOnlyCollection<ModelDefinition> availableModels,
        decimal resourceBudget)
    {
        ArgumentNullException.ThrowIfNull(job);
        if (resourceBudget <= 0m || resourceBudget > 1m)
            throw new ArgumentOutOfRangeException(nameof(resourceBudget), "Resource budget must be greater than 0 and at most 1.");

        var settings = options.Value;
        var previous = new List<PreviousModelExecution>();
        var executions = new List<AdaptiveModelExecution>();
        IReadOnlyCollection<PredictionResult> finalResults = [];
        ModelDefinition? finalModel = null;
        decimal finalConfidence = 0m;
        var remaining = resourceBudget;

        while (remaining > 0m)
        {
            ModelRoutingResult decision;
            try
            {
                decision = router.Route(new ModelRoutingContext(job.Candidates, availableModels, previous, remaining));
            }
            catch (InvalidOperationException) when (executions.Count > 0)
            {
                break;
            }

            finalModel = availableModels.Single(x => x.Id == decision.SelectedModelDefinitionId);
            finalResults = scoringEngine.Score(job, finalModel);
            finalConfidence = CalculatePrototypeConfidence(finalResults);

            var profile = settings.Models.Single(x => x.ModelDefinitionId == finalModel.Id);
            var cost = Math.Clamp(profile.EstimatedResourceCost, 0m, 1m);
            remaining = Math.Max(0m, remaining - cost);

            previous.Add(new PreviousModelExecution(finalModel.Id, finalConfidence, cost));
            executions.Add(new AdaptiveModelExecution(
                finalModel.Id, finalModel.Name, finalConfidence, cost,
                decision.RoutingScore, decision.Reason));

            if (finalConfidence >= settings.MinimumAcceptedConfidence)
                break;
        }

        if (finalModel is null)
            throw new InvalidOperationException("Adaptive routing could not select a model.");

        return new AdaptivePredictionExecution(
            finalModel, finalResults, finalConfidence,
            decimal.Round(resourceBudget - remaining, 4), executions);
    }

    private static decimal CalculatePrototypeConfidence(IReadOnlyCollection<PredictionResult> results)
    {
        if (results.Count == 0) return 0m;
        var top = results.OrderBy(x => x.Rank).Take(Math.Min(5, results.Count)).ToArray();
        var confidence = top.Average(x => Math.Abs(x.PredictionScore - 50m) / 50m);
        return decimal.Round(Math.Clamp(confidence, 0m, 1m), 4);
    }
}
