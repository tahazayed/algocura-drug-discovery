using Algocura.ClinicalResearch.Api.Contracts;

namespace Algocura.ClinicalResearch.Api.Services;

public interface IWaelCovidResearchService
{
    Task<WaelDatasetValidationResponse> ValidateDatasetsAsync(
        CancellationToken cancellationToken);

    Task<Covid19PublicHealthAnalysisResponse> AnalyzeAsync(
        Covid19PublicHealthAnalysisRequest request,
        CancellationToken cancellationToken);
}
