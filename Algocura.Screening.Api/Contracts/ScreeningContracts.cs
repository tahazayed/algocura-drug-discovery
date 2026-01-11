using Algocura.Screening.Api.Domain;

namespace Algocura.Screening.Api.Contracts;

public sealed record CreateScreeningRequest(string Name, string ScreeningMethod, string? Description, IReadOnlyCollection<Guid> CompoundIds);
public sealed record UpdateScreeningRequest(string Name, string ScreeningMethod, string? Description);
public sealed record CompleteScreeningRequest(IReadOnlyCollection<ScreeningResultInput> Results);
public sealed record ScreeningResultInput(Guid CompoundId, decimal Score, bool Selected, string? Notes);
public sealed record FailScreeningRequest(string Reason);

public sealed record ScreeningCandidateResponse(Guid CompoundId, string CompoundName, string CanonicalSmiles);
public sealed record ScreeningResultResponse(Guid CompoundId, decimal Score, int Rank, bool Selected, string? Notes);
public sealed record ScreeningResponse(
    Guid Id,
    string Name,
    string ScreeningMethod,
    string? Description,
    ScreeningStatus Status,
    DateTimeOffset CreatedAtUtc,
    DateTimeOffset UpdatedAtUtc,
    DateTimeOffset? StartedAtUtc,
    DateTimeOffset? CompletedAtUtc,
    string? FailureReason,
    IReadOnlyCollection<ScreeningCandidateResponse> Candidates,
    IReadOnlyCollection<ScreeningResultResponse> Results);
