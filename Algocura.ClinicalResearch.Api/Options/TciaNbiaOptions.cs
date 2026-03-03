namespace Algocura.ClinicalResearch.Api.Options;

public sealed class TciaNbiaOptions
{
    public const string SectionName = "TciaNbia";
    public string BaseUrl { get; set; } = "https://services.cancerimagingarchive.net/nbia-api/services/v1/";
    public int TimeoutSeconds { get; set; } = 120;
    public int MaxSeriesDownloadMegabytes { get; set; } = 1024;
}
