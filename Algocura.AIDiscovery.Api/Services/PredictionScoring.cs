using Algocura.AIDiscovery.Api.Domain;

namespace Algocura.AIDiscovery.Api.Services;

public interface IPredictionScoringEngine
{
    IReadOnlyCollection<PredictionResult> Score(
        PredictionJob job,
        ModelDefinition model);
}

public sealed class DeterministicPredictionScoringEngine
    : IPredictionScoringEngine
{
    public IReadOnlyCollection<PredictionResult> Score(
        PredictionJob job,
        ModelDefinition model)
    {
        var scored = job.Candidates
            .Select(candidate =>
            {
                var weightScore =
                    candidate.MolecularWeight <= 0
                        ? 0m
                        : Math.Clamp(
                            100m -
                            Math.Abs(candidate.MolecularWeight - 350m) / 3.5m,
                            0m,
                            100m);

                var score =
                    candidate.ScreeningScore * model.ScreeningScoreWeight +
                    weightScore * model.MolecularWeightWeight +
                    (candidate.ScreeningSelected
                        ? model.SelectionBonus
                        : 0m);

                return new
                {
                    Candidate = candidate,
                    Score = decimal.Round(
                        Math.Clamp(score, 0m, 100m),
                        4)
                };
            })
            .OrderByDescending(x => x.Score)
            .ThenBy(x => x.Candidate.CompoundName)
            .ToArray();

        return scored
            .Select((item, index) =>
                new PredictionResult
                {
                    Id = Guid.NewGuid(),
                    PredictionJobId = job.Id,
                    CompoundId = item.Candidate.CompoundId,
                    PredictionScore = item.Score,
                    Rank = index + 1,
                    Prioritized = index < Math.Min(5, scored.Length),
                    Explanation =
                        $"Screening score={item.Candidate.ScreeningScore}; " +
                        $"molecular weight={item.Candidate.MolecularWeight}; " +
                        $"selected={item.Candidate.ScreeningSelected}."
                })
            .ToArray();
    }
}
