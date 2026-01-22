namespace Algocura.AIDiscovery.Api.Routing;

public sealed class ModelRoutingOptions
{
    public const string SectionName = "AdaptiveRouting";
    public bool Enabled { get; set; } = true;
    public decimal MinimumAcceptedConfidence { get; set; } = 0.80m;
    public decimal MolecularSuitabilityWeight { get; set; } = 0.35m;
    public decimal ExpectedConfidenceWeight { get; set; } = 0.35m;
    public decimal SpecializationWeight { get; set; } = 0.20m;
    public decimal ResourceCostWeight { get; set; } = 0.10m;
    public List<ModelRoutingProfile> Models { get; set; } = [];
}

public sealed class ModelRoutingProfile
{
    public Guid ModelDefinitionId { get; set; }
    public decimal PreferredMolecularWeight { get; set; } = 350m;
    public decimal MolecularWeightTolerance { get; set; } = 250m;
    public decimal ExpectedConfidence { get; set; } = 0.75m;
    public decimal SpecializationScore { get; set; } = 0.50m;
    public decimal EstimatedResourceCost { get; set; } = 0.50m;
    public int EstimatedLatencyMilliseconds { get; set; } = 100;
}
