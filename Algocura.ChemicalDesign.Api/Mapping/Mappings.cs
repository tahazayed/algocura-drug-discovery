using Algocura.ChemicalDesign.Api.Contracts;
using Algocura.ChemicalDesign.Api.Domain;

namespace Algocura.ChemicalDesign.Api.Mapping;

public static class Mappings
{
    public static SelectedTargetResponse ToResponse(this SelectedTargetProjection x) =>
        new(x.TargetAssessmentId, x.TargetHypothesisId, x.DiseaseName,
            x.GeneSymbol, x.ProteinTarget, x.DesiredModulation, x.Rationale,
            x.LiteratureEvidenceScore, x.TargetAssessmentScore, x.ConfidenceLevel,
            x.SourceUpdatedAtUtc, x.SynchronizedAtUtc);

    public static QueryStructureResponse ToResponse(this QueryStructure x) =>
        new(x.Id, x.DesignProjectId, x.Name, x.Structure, x.StructureFormat,
            x.SearchPurpose, x.CreatedAtUtc);

    public static CandidateStructureResponse ToResponse(this CandidateStructure x) =>
        new(x.Id, x.DesignProjectId, x.StructureSearchJobId, x.Name,
            x.CanonicalSmiles, x.InchiKey, x.MolecularFormula,
            x.MolecularWeight, x.MatchScore, x.SourceReference,
            x.SelectionRationale, x.Status, x.CreatedAtUtc, x.UpdatedAtUtc,
            x.SelectedAtUtc);

    public static StructureSearchResponse ToResponse(this StructureSearchJob x) =>
        new(x.Id, x.DesignProjectId, x.QueryStructureId, x.SearchType,
            x.SimilarityThreshold, x.MaximumResults, x.Provider, x.Status,
            x.FailureReason, x.CreatedAtUtc, x.UpdatedAtUtc, x.StartedAtUtc,
            x.CompletedAtUtc, x.Candidates.OrderByDescending(c => c.MatchScore)
                .Select(c => c.ToResponse()).ToArray());

    public static SynthesisRouteResponse ToResponse(this SynthesisRoute x) =>
        new(x.Id, x.CandidateStructureId, x.Name, x.Description,
            x.EstimatedYieldPercent, x.StepCount, x.Status, x.CreatedAtUtc,
            x.UpdatedAtUtc, x.Steps.OrderBy(s => s.StepNumber)
                .Select(s => new ReactionStepResponse(
                    s.Id, s.StepNumber, s.ReactionName, s.Reactants,
                    s.Reagents, s.Conditions, s.ProductSmiles,
                    s.EstimatedYieldPercent)).ToArray());

    public static DesignProjectResponse ToResponse(this ChemicalDesignProject x) =>
        new(x.Id, x.TargetAssessmentId, x.Name, x.Objective,
            x.DesiredModulation, x.Status, x.CreatedAtUtc, x.UpdatedAtUtc,
            x.CompletedAtUtc, x.QueryStructures.Select(q => q.ToResponse()).ToArray(),
            x.SearchJobs.Select(s => s.ToResponse()).ToArray(),
            x.Candidates.OrderByDescending(c => c.MatchScore)
                .Select(c => c.ToResponse()).ToArray());
}
