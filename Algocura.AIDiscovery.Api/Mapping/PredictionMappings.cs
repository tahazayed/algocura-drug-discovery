using Algocura.AIDiscovery.Api.Contracts;
using Algocura.AIDiscovery.Api.Domain;

namespace Algocura.AIDiscovery.Api.Mapping;

public static class PredictionMappings
{
    public static PredictionResponse ToResponse(this PredictionJob job)
    {
        return new PredictionResponse(
            job.Id,
            job.Name,
            job.ModelDefinitionId,
            job.SourceScreeningId,
            job.Description,
            job.Status,
            job.FailureReason,
            job.CreatedAtUtc,
            job.UpdatedAtUtc,
            job.StartedAtUtc,
            job.CompletedAtUtc,
            job.Candidates
                .OrderBy(x => x.ScreeningRank)
                .Select(x => new PredictionCandidateResponse(
                    x.CompoundId,
                    x.CompoundName,
                    x.ScreeningScore,
                    x.ScreeningRank,
                    x.ScreeningSelected,
                    x.MolecularWeight))
                .ToArray(),
            job.Results
                .OrderBy(x => x.Rank)
                .Select(x => new PredictionResultResponse(
                    x.CompoundId,
                    x.PredictionScore,
                    x.Rank,
                    x.Prioritized,
                    x.Explanation))
                .ToArray());
    }

    public static ModelDefinitionResponse ToResponse(
        this ModelDefinition model)
    {
        return new ModelDefinitionResponse(
            model.Id,
            model.Name,
            model.Version,
            model.Description,
            model.ScreeningScoreWeight,
            model.MolecularWeightWeight,
            model.SelectionBonus,
            model.Status);
    }
}
