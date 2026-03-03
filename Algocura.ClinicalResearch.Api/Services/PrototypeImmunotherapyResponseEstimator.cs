using Algocura.ClinicalResearch.Api.Contracts;

namespace Algocura.ClinicalResearch.Api.Services;

public sealed class PrototypeImmunotherapyResponseEstimator : IImmunotherapyResponseEstimator
{
    public ImmunotherapyResponseEstimateResponse Estimate(ImmunotherapyResponseEstimateRequest request)
    {
        // Research/demo-only logistic regression. Coefficients are deliberately illustrative and are NOT clinically fitted.
        var age = Math.Clamp(request.Age / 100d, 0d, 1d);
        var contributions = new Dictionary<string, double>
        {
            ["pdL1"] = 1.25 * request.NormalizedPdL1,
            ["tumorMutationalBurden"] = 1.05 * request.NormalizedTmb,
            ["immuneInfiltration"] = 1.10 * request.NormalizedImmuneInfiltration,
            ["tumorBurden"] = -0.85 * request.NormalizedTumorBurden,
            ["priorTreatmentBurden"] = -0.45 * request.PriorTreatmentBurden,
            ["age"] = -0.20 * age
        };

        const double intercept = -0.35;
        var logit = intercept + contributions.Values.Sum();
        var score = 1d / (1d + Math.Exp(-logit));

        return new ImmunotherapyResponseEstimateResponse(
            Math.Round(score, 4),
            "Algocura Experimental Immunotherapy Response Logistic Model",
            "prototype-0.1",
            contributions.ToDictionary(x => x.Key, x => Math.Round(x.Value, 4)),
            "Research prototype only. Coefficients are illustrative, not clinically fitted or validated, and this score must not be used for diagnosis, prognosis, or treatment decisions.");
    }
}
