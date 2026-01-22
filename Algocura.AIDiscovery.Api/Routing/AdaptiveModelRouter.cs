using Algocura.AIDiscovery.Api.Domain;
using Microsoft.Extensions.Options;

namespace Algocura.AIDiscovery.Api.Routing;

public sealed class AdaptiveModelRouter(IOptions<ModelRoutingOptions> options) : IAdaptiveModelRouter
{
    public ModelRoutingResult Route(ModelRoutingContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        var settings = options.Value;

        if (!settings.Enabled)
            throw new InvalidOperationException("Adaptive model routing is disabled.");
        if (context.Candidates.Count == 0)
            throw new InvalidOperationException("At least one prediction candidate is required.");

        var executed = context.PreviousExecutions.Select(x => x.ModelDefinitionId).ToHashSet();
        var profiles = settings.Models.ToDictionary(x => x.ModelDefinitionId);
        var averageMw = context.Candidates.Average(x => x.MolecularWeight);

        var scored = context.AvailableModels
            .Where(x => x.Status == ModelStatus.Active && !executed.Contains(x.Id) && profiles.ContainsKey(x.Id))
            .Select(model =>
            {
                var p = profiles[model.Id];
                var cost = Math.Clamp(p.EstimatedResourceCost, 0m, 1m);
                if (cost > context.RemainingResourceBudget) return null;

                var tolerance = Math.Max(p.MolecularWeightTolerance, 1m);
                var suitability = Math.Clamp(1m - Math.Abs(averageMw - p.PreferredMolecularWeight) / tolerance, 0m, 1m);
                var confidence = Math.Clamp(p.ExpectedConfidence, 0m, 1m);
                var specialization = Math.Clamp(p.SpecializationScore, 0m, 1m);
                var score =
                    suitability * settings.MolecularSuitabilityWeight +
                    confidence * settings.ExpectedConfidenceWeight +
                    specialization * settings.SpecializationWeight -
                    cost * settings.ResourceCostWeight;

                return new ModelRoutingCandidateScore(
                    model.Id, model.Name,
                    decimal.Round(suitability, 4),
                    decimal.Round(confidence, 4),
                    decimal.Round(specialization, 4),
                    decimal.Round(cost, 4),
                    decimal.Round(score, 4));
            })
            .Where(x => x is not null)
            .Cast<ModelRoutingCandidateScore>()
            .OrderByDescending(x => x.RoutingScore)
            .ThenBy(x => x.ModelName)
            .ToArray();

        if (scored.Length == 0)
            throw new InvalidOperationException("No eligible active model fits the remaining resource budget.");

        var selected = scored[0];
        var prior = context.PreviousExecutions.Count == 0 ? (decimal?)null : context.PreviousExecutions.Max(x => x.Confidence);
        var reason = prior.HasValue
            ? $"Selected {selected.ModelName} after previous confidence {prior.Value:0.0000}; routing score={selected.RoutingScore:0.0000}."
            : $"Selected {selected.ModelName} from molecular suitability, expected confidence, specialization, and resource cost; routing score={selected.RoutingScore:0.0000}.";

        return new ModelRoutingResult(selected.ModelDefinitionId, selected.ModelName, selected.RoutingScore, reason, scored);
    }
}
