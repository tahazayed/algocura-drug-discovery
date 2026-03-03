namespace Algocura.ClinicalResearch.Api.Events;

public sealed record FollowUpCtSimulated : IntegrationEvent
{
    public required double ResearchResponseScore { get; init; }
    public required double AppliedShrinkageFraction { get; init; }
    public required int OriginalLesionRadiusPixels { get; init; }
    public required int SimulatedLesionRadiusPixels { get; init; }
}
