using Algocura.ClinicalResearch.Api.Contracts;

namespace Algocura.ClinicalResearch.Api.Services;

public interface ITciaNbiaClient
{
    Task<IReadOnlyCollection<TciaPatientResponse>> GetPatientsAsync(string collection, string? modality, CancellationToken cancellationToken);
    Task<IReadOnlyCollection<TciaSeriesResponse>> GetSeriesAsync(string collection, string patientId, CancellationToken cancellationToken);
    Task<byte[]> DownloadSeriesZipAsync(string seriesInstanceUid, CancellationToken cancellationToken);
}
