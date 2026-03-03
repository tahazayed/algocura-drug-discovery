namespace Algocura.ClinicalResearch.Api.Events;

public sealed record Covid19PublicHealthAnalysisCompleted : IntegrationEvent
{
    public required Guid AnalysisId { get; init; }
    public required string CountryCode { get; init; }
    public required DateOnly StartDate { get; init; }
    public required DateOnly EndDate { get; init; }
    public required int RegionsMatched { get; init; }
    public required int FusedDailyRecords { get; init; }
}
