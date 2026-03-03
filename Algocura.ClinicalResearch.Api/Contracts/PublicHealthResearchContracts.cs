using System.ComponentModel.DataAnnotations;

namespace Algocura.ClinicalResearch.Api.Contracts;

public sealed record WaelDatasetValidationItem(
    string StudyCode,
    string Name,
    string SourceName,
    string? SourceUri,
    string Status,
    bool Registered,
    bool Reachable,
    long? ContentLength,
    string? ContentType,
    string? Error);

public sealed record WaelDatasetValidationResponse(
    bool Ready,
    IReadOnlyCollection<WaelDatasetValidationItem> Datasets,
    string Disclaimer);

public sealed record Covid19PublicHealthAnalysisRequest
{
    [Required, StringLength(2, MinimumLength = 2)]
    public string CountryCode { get; init; } = "US";

    [Required]
    public DateOnly StartDate { get; init; } = new(2021, 1, 1);

    [Required]
    public DateOnly EndDate { get; init; } = new(2021, 3, 31);

    [Range(1, 3)]
    public int GoogleAggregationLevel { get; init; } = 1;

    [Range(1, 500)]
    public int MaximumRegions { get; init; } = 60;

    [Range(0, 1_000_000)]
    public int AvailableVentilators { get; init; } = 500;

    [Range(0, 1_000_000_000)]
    public int AvailablePpeUnits { get; init; } = 100_000;

    [Range(25, 5000)]
    public int SimulationIterations { get; init; } = 250;

    [Range(0, 1)]
    public double VentilatorDemandPerCase { get; init; } = 0.02;

    [Range(0, 100)]
    public double VentilatorDemandPerDeath { get; init; } = 0.5;

    [Range(0, 10_000)]
    public double PpeUnitsPerCase { get; init; } = 10;

    [Range(0, 100_000)]
    public double PpeUnitsPerDeath { get; init; } = 50;
}

public sealed record Covid19DatasetUsage(
    string StudyCode,
    Guid DatasetId,
    string Name,
    string SourceName,
    string SourceUri,
    int RecordsRead,
    int RecordsUsed);

public sealed record Covid19CountryContext(
    string CountryCode,
    long WhoNewCases,
    long WhoNewDeaths,
    long WhoCumulativeCases,
    long WhoCumulativeDeaths,
    double SevenDayCaseGrowthRate,
    double SevenDayDeathGrowthRate);

public sealed record Covid19RegionalSignal(
    string RegionKey,
    string RegionName,
    string? RegionCode,
    int AggregationLevel,
    double? Latitude,
    double? Longitude,
    long LocalNewCases,
    long LocalNewDeaths,
    double SevenDayCaseGrowthRate,
    double SevenDayDeathGrowthRate,
    double RiskScore,
    double ExpectedVentilatorDemand,
    double ExpectedPpeDemand,
    int AllocatedVentilators,
    int AllocatedPpeUnits);

public sealed record Covid19PublicHealthAnalysisResponse(
    Guid AnalysisId,
    DateTimeOffset GeneratedAtUtc,
    string CountryCode,
    DateOnly StartDate,
    DateOnly EndDate,
    int GoogleAggregationLevel,
    int RegionsMatched,
    int FusedDailyRecords,
    Covid19CountryContext WhoCountryContext,
    IReadOnlyCollection<Covid19RegionalSignal> RegionalSignals,
    IReadOnlyCollection<Covid19DatasetUsage> Datasets,
    Covid19SimulationParameters Simulation,
    string Methodology,
    string Disclaimer);

public sealed record Covid19SimulationParameters(
    int Iterations,
    int RandomSeed,
    int AvailableVentilators,
    int AvailablePpeUnits,
    double VentilatorDemandPerCase,
    double VentilatorDemandPerDeath,
    double PpeUnitsPerCase,
    double PpeUnitsPerDeath);
