using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Algocura.ClinicalResearch.Api.Controllers;

[ApiController]
[Route("api/v1/public-health-research")]
[Authorize]
public sealed class PublicHealthResearchController(
    IWaelCovidResearchService service,
    IEventPublisher publisher) : ControllerBase
{
    [HttpGet("wael/datasets/validate")]
    [ProducesResponseType<WaelDatasetValidationResponse>(StatusCodes.Status200OK)]
    public async Task<ActionResult<WaelDatasetValidationResponse>> ValidateWaelDatasets(
        CancellationToken cancellationToken)
    {
        return Ok(await service.ValidateDatasetsAsync(cancellationToken));
    }

    [HttpPost("covid19/analyze")]
    [ProducesResponseType<Covid19PublicHealthAnalysisResponse>(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<Covid19PublicHealthAnalysisResponse>> AnalyzeCovid19(
        Covid19PublicHealthAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        try
        {
            var response = await service.AnalyzeAsync(request, cancellationToken);

            await publisher.PublishAsync(
                new Covid19PublicHealthAnalysisCompleted
                {
                    AnalysisId = response.AnalysisId,
                    CountryCode = response.CountryCode,
                    StartDate = response.StartDate,
                    EndDate = response.EndDate,
                    RegionsMatched = response.RegionsMatched,
                    FusedDailyRecords = response.FusedDailyRecords
                },
                HttpContext?.TraceIdentifier,
                cancellationToken);

            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid research-analysis request",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Research datasets are not ready",
                Detail = ex.Message,
                Status = StatusCodes.Status400BadRequest
            });
        }
    }
}
