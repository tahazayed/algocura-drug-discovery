using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Events;
using Algocura.ClinicalResearch.Api.Services;
using Algocura.Platform.Messaging.Publishing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Algocura.ClinicalResearch.Api.Controllers;

[ApiController]
[Route("api/v1/imaging-research")]
[Authorize]
public sealed class ImagingResearchController(
    ITciaNbiaClient tcia,
    IImmunotherapyResponseEstimator estimator,
    IFollowUpCtSimulator simulator,
    IEventPublisher publisher,
    ILogger<ImagingResearchController> logger) : ControllerBase
{
    [HttpGet("tcia/patients")]
    public async Task<ActionResult<IReadOnlyCollection<TciaPatientResponse>>> GetTciaPatients(
        [FromQuery] string collection,
        [FromQuery] string? modality,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(collection))
            return BadRequest("collection is required.");

        var patients = await tcia.GetPatientsAsync(
            collection.Trim(),
            string.IsNullOrWhiteSpace(modality) ? null : modality.Trim(),
            cancellationToken);

        return Ok(patients);
    }

    [HttpGet("tcia/series")]
    public async Task<ActionResult<IReadOnlyCollection<TciaSeriesResponse>>> GetTciaSeries(
        [FromQuery] string collection,
        [FromQuery] string patientId,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(collection) || string.IsNullOrWhiteSpace(patientId))
            return BadRequest("collection and patientId are required.");
        return Ok(await tcia.GetSeriesAsync(collection.Trim(), patientId.Trim(), cancellationToken));
    }

    [HttpPost("tcia/curate")]
    public async Task<ActionResult<CuratedImagingSeriesResponse>> CurateTciaSeries(
        CuratedImagingSeriesRequest request,
        CancellationToken cancellationToken)
    {
        var all = await tcia.GetSeriesAsync(request.Collection, request.PatientId, cancellationToken);
        var curated = all.Where(x =>
            (string.IsNullOrWhiteSpace(request.Modality) || string.Equals(x.Modality, request.Modality, StringComparison.OrdinalIgnoreCase)) &&
            (string.IsNullOrWhiteSpace(request.BodyPartContains) || (x.BodyPartExamined?.Contains(request.BodyPartContains, StringComparison.OrdinalIgnoreCase) ?? false)) &&
            (x.ImageCount ?? 0) >= request.MinimumImageCount).ToArray();

        var response = new CuratedImagingSeriesResponse(all.Count, curated.Length, curated);
        await publisher.PublishAsync(new ImagingSeriesRetrieved
        {
            Collection = request.Collection,
            PatientId = request.PatientId,
            RetrievedCount = all.Count,
            CuratedCount = curated.Length
        }, null, cancellationToken);
        return Ok(response);
    }

    [HttpGet("tcia/series/{seriesInstanceUid}/download")]
    public async Task<IActionResult> DownloadTciaSeries(string seriesInstanceUid, CancellationToken cancellationToken)
    {
        var bytes = await tcia.DownloadSeriesZipAsync(seriesInstanceUid, cancellationToken);
        return File(bytes, "application/zip", $"tcia-{Sanitize(seriesInstanceUid)}.zip");
    }

    [HttpPost("immunotherapy-response/estimate")]
    public async Task<ActionResult<ImmunotherapyResponseEstimateResponse>> EstimateResponse(
        ImmunotherapyResponseEstimateRequest request,
        CancellationToken cancellationToken)
    {
        var response = estimator.Estimate(request);
        await publisher.PublishAsync(new ImmunotherapyResponseEstimated
        {
            CancerType = request.CancerType,
            SeriesInstanceUid = request.SeriesInstanceUid,
            ResearchResponseScore = response.ResearchResponseScore,
            ModelVersion = response.ModelVersion
        }, null, cancellationToken);
        return Ok(response);
    }

    [HttpPost("simulate-followup-ct")]
    [RequestSizeLimit(25 * 1024 * 1024)]
    public async Task<IActionResult> SimulateFollowUpCt(
        [FromForm] IFormFile image,
        [FromForm] double responseScore,
        [FromForm] int lesionCenterX,
        [FromForm] int lesionCenterY,
        [FromForm] int lesionRadiusPixels,
        CancellationToken cancellationToken)
    {
        if (image.Length == 0) return BadRequest("A rendered CT image is required.");
        if (image.ContentType is not ("image/png" or "image/jpeg"))
            return BadRequest("For this prototype, upload a rendered CT slice as PNG or JPEG.");

        await using var input = image.OpenReadStream();
        var result = await simulator.SimulateAsync(input, responseScore, lesionCenterX, lesionCenterY, lesionRadiusPixels, cancellationToken);
        Response.Headers.Append("X-Algocura-Research-Disclaimer", result.Metadata.Disclaimer);
        Response.Headers.Append("X-Algocura-Applied-Shrinkage", result.Metadata.AppliedShrinkageFraction.ToString("0.####", System.Globalization.CultureInfo.InvariantCulture));

        await publisher.PublishAsync(new FollowUpCtSimulated
        {
            ResearchResponseScore = responseScore,
            AppliedShrinkageFraction = result.Metadata.AppliedShrinkageFraction,
            OriginalLesionRadiusPixels = result.Metadata.OriginalLesionRadiusPixels,
            SimulatedLesionRadiusPixels = result.Metadata.SimulatedLesionRadiusPixels
        }, null, cancellationToken);

        logger.LogInformation("Generated research-only simulated follow-up CT visualization. Score {Score}, radius {Original}->{New}.",
            responseScore, result.Metadata.OriginalLesionRadiusPixels, result.Metadata.SimulatedLesionRadiusPixels);
        return File(result.Png, result.Metadata.ContentType, result.Metadata.FileName);
    }

    private static string Sanitize(string value) => string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_' ? ch : '_'));
}
