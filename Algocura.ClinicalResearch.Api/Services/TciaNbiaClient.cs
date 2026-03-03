using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Options;
using Microsoft.Extensions.Options;
using System.Globalization;
using System.Net.Http.Json;
using System.Text.Json.Serialization;

namespace Algocura.ClinicalResearch.Api.Services;

public sealed class TciaNbiaClient(HttpClient httpClient, IOptions<TciaNbiaOptions> options) : ITciaNbiaClient
{
    private readonly TciaNbiaOptions settings = options.Value;

    public async Task<IReadOnlyCollection<TciaPatientResponse>> GetPatientsAsync(
        string collection,
        string? modality,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(collection))
            throw new ArgumentException("Collection is required.", nameof(collection));

        // TCIA/NBIA exposes a patient-list endpoint that is ideal for demo discovery.
        // When modality is supplied we use getPatientByCollectionAndModality so callers
        // can discover only subjects that have the requested imaging modality.
        var url = string.IsNullOrWhiteSpace(modality)
            ? $"getPatient?Collection={Uri.EscapeDataString(collection)}&format=json"
            : $"getPatientByCollectionAndModality?Collection={Uri.EscapeDataString(collection)}&Modality={Uri.EscapeDataString(modality)}&format=json";

        var rows = await httpClient.GetFromJsonAsync<List<NbiaPatientDto>>(url, cancellationToken) ?? [];

        return rows
            .Select(x => new TciaPatientResponse(
                x.Collection ?? collection,
                x.PatientId ?? string.Empty))
            .Where(x => !string.IsNullOrWhiteSpace(x.PatientId))
            .GroupBy(x => x.PatientId, StringComparer.OrdinalIgnoreCase)
            .Select(g => g.First())
            .OrderBy(x => x.PatientId, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }

    public async Task<IReadOnlyCollection<TciaSeriesResponse>> GetSeriesAsync(string collection, string patientId, CancellationToken cancellationToken)
    {
        var url = $"getSeries?Collection={Uri.EscapeDataString(collection)}&PatientID={Uri.EscapeDataString(patientId)}&format=json";
        var rows = await httpClient.GetFromJsonAsync<List<NbiaSeriesDto>>(url, cancellationToken) ?? [];
        return rows.Select(x => new TciaSeriesResponse(
            x.Collection ?? collection,
            x.PatientId ?? patientId,
            x.StudyInstanceUid ?? string.Empty,
            x.SeriesInstanceUid ?? string.Empty,
            x.Modality,
            x.BodyPartExamined,
            x.SeriesDescription,
            int.TryParse(x.ImageCount, NumberStyles.Integer, CultureInfo.InvariantCulture, out var count) ? count : null,
            x.StudyDate)).Where(x => !string.IsNullOrWhiteSpace(x.SeriesInstanceUid)).ToArray();
    }

    public async Task<byte[]> DownloadSeriesZipAsync(string seriesInstanceUid, CancellationToken cancellationToken)
    {
        using var response = await httpClient.GetAsync($"getImage?SeriesInstanceUID={Uri.EscapeDataString(seriesInstanceUid)}", HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        var length = response.Content.Headers.ContentLength;
        var max = settings.MaxSeriesDownloadMegabytes * 1024L * 1024L;
        if (length.HasValue && length.Value > max)
            throw new InvalidOperationException($"TCIA series exceeds configured maximum download size of {settings.MaxSeriesDownloadMegabytes} MB.");
        var bytes = await response.Content.ReadAsByteArrayAsync(cancellationToken);
        if (bytes.LongLength > max)
            throw new InvalidOperationException($"TCIA series exceeds configured maximum download size of {settings.MaxSeriesDownloadMegabytes} MB.");
        return bytes;
    }

    private sealed record NbiaPatientDto
    {
        [JsonPropertyName("Collection")] public string? Collection { get; init; }

        // NBIA documentation generally shows PatientId, while some responses use PatientID.
        // System.Text.Json property matching is case-insensitive by default for Web defaults,
        // but retaining the documented spelling here keeps the model explicit.
        [JsonPropertyName("PatientId")] public string? PatientId { get; init; }
    }

    private sealed record NbiaSeriesDto
    {
        [JsonPropertyName("Collection")] public string? Collection { get; init; }
        [JsonPropertyName("PatientID")] public string? PatientId { get; init; }
        [JsonPropertyName("StudyInstanceUID")] public string? StudyInstanceUid { get; init; }
        [JsonPropertyName("SeriesInstanceUID")] public string? SeriesInstanceUid { get; init; }
        [JsonPropertyName("Modality")] public string? Modality { get; init; }
        [JsonPropertyName("BodyPartExamined")] public string? BodyPartExamined { get; init; }
        [JsonPropertyName("SeriesDescription")] public string? SeriesDescription { get; init; }
        [JsonPropertyName("ImageCount")] public string? ImageCount { get; init; }
        [JsonPropertyName("StudyDate")] public string? StudyDate { get; init; }
    }
}
