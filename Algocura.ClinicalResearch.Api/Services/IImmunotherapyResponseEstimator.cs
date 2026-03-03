using Algocura.ClinicalResearch.Api.Contracts;

namespace Algocura.ClinicalResearch.Api.Services;

public interface IImmunotherapyResponseEstimator
{
    ImmunotherapyResponseEstimateResponse Estimate(ImmunotherapyResponseEstimateRequest request);
}
