using Algocura.Screening.Api.Contracts;
using Algocura.Screening.Api.Domain;

namespace Algocura.Screening.Api.Mapping;

public static class ScreeningMapping
{
    public static ScreeningResponse ToResponse(this ScreeningJob job) => new(
        job.Id, job.Name, job.ScreeningMethod, job.Description, job.Status,
        job.CreatedAtUtc, job.UpdatedAtUtc, job.StartedAtUtc, job.CompletedAtUtc, job.FailureReason,
        job.Candidates.Select(x => new ScreeningCandidateResponse(x.CompoundId, x.CompoundName, x.CanonicalSmiles)).ToArray(),
        job.Results.OrderBy(x => x.Rank).Select(x => new ScreeningResultResponse(x.CompoundId, x.Score, x.Rank, x.Selected, x.Notes)).ToArray());
}
