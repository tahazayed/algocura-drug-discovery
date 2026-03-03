namespace Algocura.ClinicalResearch.Api.Events;

public sealed record ResearchSubjectsImported : IntegrationEvent
{
    public required Guid ClinicalDatasetId { get; init; }
    public required string DatasetName { get; init; }
    public required string SourceName { get; init; }
    public string? StudyCode { get; init; }
    public required int InsertedCount { get; init; }
    public required int UpdatedCount { get; init; }
    public required int TotalSubjectCount { get; init; }
}
