using Algocura.ClinicalResearch.Api.Contracts;
using Algocura.ClinicalResearch.Api.Data;
using Algocura.ClinicalResearch.Api.Domain;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using System.Net.Http.Headers;

namespace Algocura.ClinicalResearch.Api.Services;

/// <summary>
/// Research-only retrospective COVID-19 data-fusion prototype based on the
/// WHO COVID-19 downloadable statistical release and Google COVID-19 Open Data.
/// This service is intentionally deterministic/reproducible and is not a
/// validated epidemiological forecasting or clinical decision-support model.
/// </summary>
public sealed class WaelCovidResearchService(
    ClinicalResearchDbContext db,
    IHttpClientFactory httpClientFactory,
    ILogger<WaelCovidResearchService> logger) : IWaelCovidResearchService
{
    public const string WhoStudyCode = "WHO-COVID19-DAILY";
    public const string GoogleEpidemiologyStudyCode = "GOOGLE-COVID19-EPI";
    public const string GoogleGeographicIndexStudyCode = "GOOGLE-COVID19-GEO";

    private const int DeterministicSimulationSeed = 20260907;

    private static readonly string[] RequiredStudyCodes =
    [
        WhoStudyCode,
        GoogleEpidemiologyStudyCode,
        GoogleGeographicIndexStudyCode
    ];

    public async Task<WaelDatasetValidationResponse> ValidateDatasetsAsync(
        CancellationToken cancellationToken)
    {
        var registered = await db.ClinicalDatasets
            .AsNoTracking()
            .Where(x => x.StudyCode != null && RequiredStudyCodes.Contains(x.StudyCode))
            .ToListAsync(cancellationToken);

        var client = httpClientFactory.CreateClient("WaelPublicHealthData");
        var results = new List<WaelDatasetValidationItem>(RequiredStudyCodes.Length);

        foreach (var studyCode in RequiredStudyCodes)
        {
            var dataset = registered.FirstOrDefault(x =>
                string.Equals(x.StudyCode, studyCode, StringComparison.OrdinalIgnoreCase));

            if (dataset is null)
            {
                results.Add(new WaelDatasetValidationItem(
                    studyCode,
                    studyCode,
                    "Not registered",
                    null,
                    "Missing",
                    false,
                    false,
                    null,
                    null,
                    "Dataset is not registered in Clinical Research."));
                continue;
            }

            if (string.IsNullOrWhiteSpace(dataset.SourceUri))
            {
                results.Add(new WaelDatasetValidationItem(
                    studyCode,
                    dataset.Name,
                    dataset.SourceName,
                    dataset.SourceUri,
                    dataset.Status.ToString(),
                    true,
                    false,
                    null,
                    null,
                    "Registered dataset has no SourceUri."));
                continue;
            }

            try
            {
                using var request = new HttpRequestMessage(HttpMethod.Get, dataset.SourceUri);
                request.Headers.Range = new RangeHeaderValue(0, 0);

                using var response = await client.SendAsync(
                    request,
                    HttpCompletionOption.ResponseHeadersRead,
                    cancellationToken);

                results.Add(new WaelDatasetValidationItem(
                    studyCode,
                    dataset.Name,
                    dataset.SourceName,
                    dataset.SourceUri,
                    dataset.Status.ToString(),
                    true,
                    response.IsSuccessStatusCode,
                    response.Content.Headers.ContentLength,
                    response.Content.Headers.ContentType?.MediaType,
                    response.IsSuccessStatusCode
                        ? null
                        : $"HTTP {(int)response.StatusCode} {response.ReasonPhrase}"));
            }
            catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
            {
                results.Add(new WaelDatasetValidationItem(
                    studyCode,
                    dataset.Name,
                    dataset.SourceName,
                    dataset.SourceUri,
                    dataset.Status.ToString(),
                    true,
                    false,
                    null,
                    null,
                    ex.Message));
            }
        }

        var ready = results.All(x =>
            x.Registered &&
            x.Reachable &&
            string.Equals(x.Status, ClinicalDatasetStatus.Ready.ToString(), StringComparison.OrdinalIgnoreCase));

        return new WaelDatasetValidationResponse(
            ready,
            results,
            "Research-data provenance/reachability validation only. It does not validate the scientific accuracy of the source data or any downstream model.");
    }

    public async Task<Covid19PublicHealthAnalysisResponse> AnalyzeAsync(
        Covid19PublicHealthAnalysisRequest request,
        CancellationToken cancellationToken)
    {
        ValidateRequest(request);

        var datasets = await ResolveRequiredDatasetsAsync(cancellationToken);
        var whoDataset = datasets[WhoStudyCode];
        var googleEpiDataset = datasets[GoogleEpidemiologyStudyCode];
        var googleIndexDataset = datasets[GoogleGeographicIndexStudyCode];

        var client = httpClientFactory.CreateClient("WaelPublicHealthData");

        // Index is intentionally read first so that the very large epidemiology
        // file can be filtered while streaming instead of loaded in full.
        var indexResult = await ReadGoogleIndexAsync(
            client,
            googleIndexDataset.SourceUri!,
            request.CountryCode,
            request.GoogleAggregationLevel,
            request.MaximumRegions,
            cancellationToken);

        if (indexResult.Regions.Count == 0)
        {
            throw new InvalidOperationException(
                $"No Google geographic regions were found for country '{request.CountryCode}' at aggregation level {request.GoogleAggregationLevel}.");
        }

        var regionKeys = indexResult.Regions.Keys.ToHashSet(StringComparer.OrdinalIgnoreCase);

        var googleEpiResult = await ReadGoogleEpidemiologyAsync(
            client,
            googleEpiDataset.SourceUri!,
            regionKeys,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        var whoResult = await ReadWhoAsync(
            client,
            whoDataset.SourceUri!,
            request.CountryCode,
            request.StartDate,
            request.EndDate,
            cancellationToken);

        if (whoResult.Records.Count == 0)
        {
            throw new InvalidOperationException(
                $"WHO dataset contained no records for country '{request.CountryCode}' in the requested period.");
        }

        if (googleEpiResult.Records.Count == 0)
        {
            throw new InvalidOperationException(
                $"Google epidemiology dataset contained no matching regional records for country '{request.CountryCode}' in the requested period.");
        }

        var whoByDate = whoResult.Records.ToDictionary(x => x.Date);
        var fusedDailyCount = googleEpiResult.Records.Count(x => whoByDate.ContainsKey(x.Date));

        var whoContext = BuildWhoContext(request.CountryCode, whoResult.Records);
        var signals = BuildRegionalSignals(
            request,
            indexResult.Regions,
            googleEpiResult.Records,
            whoContext);

        var datasetUsage = new[]
        {
            new Covid19DatasetUsage(
                whoDataset.StudyCode!,
                whoDataset.Id,
                whoDataset.Name,
                whoDataset.SourceName,
                whoDataset.SourceUri!,
                whoResult.RecordsRead,
                whoResult.Records.Count),
            new Covid19DatasetUsage(
                googleEpiDataset.StudyCode!,
                googleEpiDataset.Id,
                googleEpiDataset.Name,
                googleEpiDataset.SourceName,
                googleEpiDataset.SourceUri!,
                googleEpiResult.RecordsRead,
                googleEpiResult.Records.Count),
            new Covid19DatasetUsage(
                googleIndexDataset.StudyCode!,
                googleIndexDataset.Id,
                googleIndexDataset.Name,
                googleIndexDataset.SourceName,
                googleIndexDataset.SourceUri!,
                indexResult.RecordsRead,
                indexResult.Regions.Count)
        };

        var analysisId = Guid.NewGuid();

        logger.LogInformation(
            "Completed retrospective COVID-19 research analysis {AnalysisId}. Country {CountryCode}; regions {Regions}; fused records {FusedRecords}.",
            analysisId,
            request.CountryCode,
            signals.Count,
            fusedDailyCount);

        return new Covid19PublicHealthAnalysisResponse(
            analysisId,
            DateTimeOffset.UtcNow,
            request.CountryCode.ToUpperInvariant(),
            request.StartDate,
            request.EndDate,
            request.GoogleAggregationLevel,
            signals.Count,
            fusedDailyCount,
            whoContext,
            signals,
            datasetUsage,
            new Covid19SimulationParameters(
                request.SimulationIterations,
                DeterministicSimulationSeed,
                request.AvailableVentilators,
                request.AvailablePpeUnits,
                request.VentilatorDemandPerCase,
                request.VentilatorDemandPerDeath,
                request.PpeUnitsPerCase,
                request.PpeUnitsPerDeath),
            "Retrospective spatio-temporal fusion: Google geographic keys identify regional records; Google epidemiology supplies regional case/death time series; WHO supplies country-level daily case/death context for the same dates. Regional risk combines recent local burden/growth with WHO macro trend. Resource demand is then explored with a deterministic-seed Monte Carlo simulation using user-visible illustrative coefficients.",
            "Research prototype only. Results are retrospective, illustrative, not clinically or epidemiologically validated, and must not be used to allocate real healthcare resources or make patient/public-health decisions.");
    }

    private async Task<Dictionary<string, ClinicalDataset>> ResolveRequiredDatasetsAsync(
        CancellationToken cancellationToken)
    {
        var datasets = await db.ClinicalDatasets
            .AsNoTracking()
            .Where(x => x.StudyCode != null && RequiredStudyCodes.Contains(x.StudyCode))
            .ToListAsync(cancellationToken);

        var result = new Dictionary<string, ClinicalDataset>(StringComparer.OrdinalIgnoreCase);

        foreach (var studyCode in RequiredStudyCodes)
        {
            var dataset = datasets.FirstOrDefault(x =>
                string.Equals(x.StudyCode, studyCode, StringComparison.OrdinalIgnoreCase));

            if (dataset is null)
            {
                throw new InvalidOperationException(
                    $"Required dataset '{studyCode}' is not registered. Run the Wael dataset registration PowerShell script first.");
            }

            if (dataset.Status != ClinicalDatasetStatus.Ready)
            {
                throw new InvalidOperationException(
                    $"Required dataset '{studyCode}' is registered but status is '{dataset.Status}', not Ready.");
            }

            if (string.IsNullOrWhiteSpace(dataset.SourceUri))
            {
                throw new InvalidOperationException(
                    $"Required dataset '{studyCode}' has no SourceUri.");
            }

            result[studyCode] = dataset;
        }

        return result;
    }

    private static void ValidateRequest(Covid19PublicHealthAnalysisRequest request)
    {
        if (request.StartDate > request.EndDate)
            throw new ArgumentException("StartDate must be on or before EndDate.");

        if (request.EndDate.DayNumber - request.StartDate.DayNumber > 730)
            throw new ArgumentException("For this prototype, the analysis period cannot exceed 730 days.");

        if (request.CountryCode.Length != 2)
            throw new ArgumentException("CountryCode must be an ISO 3166-1 alpha-2 code.");
    }

    private static async Task<GoogleIndexReadResult> ReadGoogleIndexAsync(
        HttpClient client,
        string uri,
        string countryCode,
        int aggregationLevel,
        int maximumRegions,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var csv = await CsvStreamReader.CreateAsync(reader, cancellationToken);
        var regions = new Dictionary<string, GoogleRegion>(StringComparer.OrdinalIgnoreCase);
        var read = 0;

        while (await csv.ReadAsync(cancellationToken) is { } row)
        {
            read++;
            if (!EqualsIgnoreCase(row, "country_code", countryCode)) continue;

            var level = ParseInt(row, "aggregation_level");
            if (level != aggregationLevel) continue;

            var key = Get(row, "location_key");
            if (string.IsNullOrWhiteSpace(key)) continue;

            var regionName = FirstNotBlank(
                Get(row, "locality_name"),
                Get(row, "subregion2_name"),
                Get(row, "subregion1_name"),
                Get(row, "country_name"),
                key);

            var regionCode = FirstNotBlank(
                Get(row, "locality_code"),
                Get(row, "subregion2_code"),
                Get(row, "subregion1_code"),
                Get(row, "country_code"));

            regions[key] = new GoogleRegion(
                key,
                regionName,
                regionCode,
                level ?? aggregationLevel,
                ParseDouble(row, "latitude"),
                ParseDouble(row, "longitude"));

            if (regions.Count >= maximumRegions)
                break;
        }

        return new GoogleIndexReadResult(read, regions);
    }

    private static async Task<GoogleEpiReadResult> ReadGoogleEpidemiologyAsync(
        HttpClient client,
        string uri,
        HashSet<string> regionKeys,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var csv = await CsvStreamReader.CreateAsync(reader, cancellationToken);
        var records = new List<GoogleEpiRecord>();
        var read = 0;

        while (await csv.ReadAsync(cancellationToken) is { } row)
        {
            read++;
            var key = Get(row, "location_key");
            if (string.IsNullOrWhiteSpace(key) || !regionKeys.Contains(key)) continue;

            if (!DateOnly.TryParseExact(
                    Get(row, "date"),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                continue;

            if (date < startDate || date > endDate) continue;

            records.Add(new GoogleEpiRecord(
                date,
                key,
                ParseLong(row, "new_confirmed"),
                ParseLong(row, "new_deceased"),
                ParseLong(row, "cumulative_confirmed"),
                ParseLong(row, "cumulative_deceased")));
        }

        return new GoogleEpiReadResult(read, records);
    }

    private static async Task<WhoReadResult> ReadWhoAsync(
        HttpClient client,
        string uri,
        string countryCode,
        DateOnly startDate,
        DateOnly endDate,
        CancellationToken cancellationToken)
    {
        using var response = await client.GetAsync(uri, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken);
        using var reader = new StreamReader(stream);

        var csv = await CsvStreamReader.CreateAsync(reader, cancellationToken);
        var records = new List<WhoRecord>();
        var read = 0;

        while (await csv.ReadAsync(cancellationToken) is { } row)
        {
            read++;
            if (!EqualsIgnoreCase(row, "Country_code", countryCode)) continue;

            if (!DateOnly.TryParseExact(
                    Get(row, "Date_reported"),
                    "yyyy-MM-dd",
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out var date))
                continue;

            if (date < startDate || date > endDate) continue;

            records.Add(new WhoRecord(
                date,
                ParseLong(row, "New_cases"),
                ParseLong(row, "New_deaths"),
                ParseLong(row, "Cumulative_cases"),
                ParseLong(row, "Cumulative_deaths")));
        }

        records.Sort((a, b) => a.Date.CompareTo(b.Date));
        return new WhoReadResult(read, records);
    }

    private static Covid19CountryContext BuildWhoContext(string countryCode, IReadOnlyList<WhoRecord> records)
    {
        var last = records[^1];
        var caseGrowth = CalculateSevenDayGrowth(records.Select(x => (x.Date, x.NewCases)).ToArray());
        var deathGrowth = CalculateSevenDayGrowth(records.Select(x => (x.Date, x.NewDeaths)).ToArray());

        return new Covid19CountryContext(
            countryCode.ToUpperInvariant(),
            records.Sum(x => Math.Max(0, x.NewCases)),
            records.Sum(x => Math.Max(0, x.NewDeaths)),
            Math.Max(0, last.CumulativeCases),
            Math.Max(0, last.CumulativeDeaths),
            caseGrowth,
            deathGrowth);
    }

    private static IReadOnlyCollection<Covid19RegionalSignal> BuildRegionalSignals(
        Covid19PublicHealthAnalysisRequest request,
        IReadOnlyDictionary<string, GoogleRegion> regions,
        IReadOnlyList<GoogleEpiRecord> records,
        Covid19CountryContext whoContext)
    {
        var grouped = records
            .GroupBy(x => x.Key, StringComparer.OrdinalIgnoreCase)
            .Where(x => regions.ContainsKey(x.Key))
            .Select(group =>
            {
                var ordered = group.OrderBy(x => x.Date).ToArray();
                var recentCases = SumRecent(ordered.Select(x => (x.Date, x.NewCases)).ToArray(), 7);
                var recentDeaths = SumRecent(ordered.Select(x => (x.Date, x.NewDeaths)).ToArray(), 7);
                var caseGrowth = CalculateSevenDayGrowth(ordered.Select(x => (x.Date, x.NewCases)).ToArray());
                var deathGrowth = CalculateSevenDayGrowth(ordered.Select(x => (x.Date, x.NewDeaths)).ToArray());
                return new RegionalWorking(
                    regions[group.Key],
                    ordered.Sum(x => Math.Max(0, x.NewCases)),
                    ordered.Sum(x => Math.Max(0, x.NewDeaths)),
                    recentCases,
                    recentDeaths,
                    caseGrowth,
                    deathGrowth);
            })
            .ToArray();

        if (grouped.Length == 0)
            return [];

        var maxRecentCases = Math.Max(1, grouped.Max(x => x.RecentCases));
        var maxRecentDeaths = Math.Max(1, grouped.Max(x => x.RecentDeaths));

        var enriched = grouped.Select(x =>
        {
            var burden =
                0.65 * (x.RecentCases / (double)maxRecentCases) +
                0.35 * (x.RecentDeaths / (double)maxRecentDeaths);

            var localTrend =
                0.65 * NormalizeGrowth(x.CaseGrowth) +
                0.35 * NormalizeGrowth(x.DeathGrowth);

            var macroTrend =
                0.65 * NormalizeGrowth(whoContext.SevenDayCaseGrowthRate) +
                0.35 * NormalizeGrowth(whoContext.SevenDayDeathGrowthRate);

            var riskScore = Math.Clamp(
                0.50 * burden +
                0.35 * localTrend +
                0.15 * macroTrend,
                0,
                1);

            var simulated = SimulateDemand(
                x.RecentCases,
                x.RecentDeaths,
                riskScore,
                request,
                x.Region.Key);

            return new RegionalSimulation(x, riskScore, simulated.Ventilators, simulated.Ppe);
        }).ToArray();

        var totalVentilatorDemand = enriched.Sum(x => x.ExpectedVentilators);
        var totalPpeDemand = enriched.Sum(x => x.ExpectedPpe);

        return enriched
            .OrderByDescending(x => x.RiskScore)
            .Select(x =>
            {
                var ventilators = ProportionalAllocation(
                    request.AvailableVentilators,
                    x.ExpectedVentilators,
                    totalVentilatorDemand);

                var ppe = ProportionalAllocation(
                    request.AvailablePpeUnits,
                    x.ExpectedPpe,
                    totalPpeDemand);

                return new Covid19RegionalSignal(
                    x.Working.Region.Key,
                    x.Working.Region.Name,
                    x.Working.Region.Code,
                    x.Working.Region.AggregationLevel,
                    x.Working.Region.Latitude,
                    x.Working.Region.Longitude,
                    x.Working.TotalCases,
                    x.Working.TotalDeaths,
                    x.Working.CaseGrowth,
                    x.Working.DeathGrowth,
                    Math.Round(x.RiskScore, 4),
                    Math.Round(x.ExpectedVentilators, 2),
                    Math.Round(x.ExpectedPpe, 2),
                    ventilators,
                    ppe);
            })
            .ToArray();
    }

    private static (double Ventilators, double Ppe) SimulateDemand(
        long recentCases,
        long recentDeaths,
        double riskScore,
        Covid19PublicHealthAnalysisRequest request,
        string regionKey)
    {
        var baseVentilators =
            Math.Max(0, recentCases) * request.VentilatorDemandPerCase +
            Math.Max(0, recentDeaths) * request.VentilatorDemandPerDeath;

        var basePpe =
            Math.Max(0, recentCases) * request.PpeUnitsPerCase +
            Math.Max(0, recentDeaths) * request.PpeUnitsPerDeath;

        var seed = DeterministicSimulationSeed ^ StableHash(regionKey);
        var random = new Random(seed);
        double ventSum = 0;
        double ppeSum = 0;

        for (var i = 0; i < request.SimulationIterations; i++)
        {
            // Box-Muller normal draw; uncertainty widens as the research risk score rises.
            var z1 = NextStandardNormal(random);
            var z2 = NextStandardNormal(random);
            var volatility = 0.10 + (0.30 * riskScore);

            ventSum += Math.Max(0, baseVentilators * (1 + volatility * z1));
            ppeSum += Math.Max(0, basePpe * (1 + volatility * z2));
        }

        return (
            ventSum / request.SimulationIterations,
            ppeSum / request.SimulationIterations);
    }

    private static double NextStandardNormal(Random random)
    {
        var u1 = 1.0 - random.NextDouble();
        var u2 = 1.0 - random.NextDouble();
        return Math.Sqrt(-2.0 * Math.Log(u1)) * Math.Cos(2.0 * Math.PI * u2);
    }

    private static int StableHash(string value)
    {
        unchecked
        {
            var hash = 17;
            foreach (var ch in value)
                hash = hash * 31 + ch;
            return hash;
        }
    }

    private static int ProportionalAllocation(int available, double demand, double totalDemand)
    {
        if (available <= 0 || demand <= 0 || totalDemand <= 0) return 0;
        return Math.Max(0, (int)Math.Round(available * (demand / totalDemand)));
    }

    private static long SumRecent((DateOnly Date, long Value)[] values, int days)
    {
        if (values.Length == 0) return 0;
        var lastDate = values.Max(x => x.Date);
        var from = lastDate.AddDays(-(days - 1));
        return values.Where(x => x.Date >= from).Sum(x => Math.Max(0, x.Value));
    }

    private static double CalculateSevenDayGrowth((DateOnly Date, long Value)[] values)
    {
        if (values.Length == 0) return 0;
        var lastDate = values.Max(x => x.Date);
        var currentFrom = lastDate.AddDays(-6);
        var previousFrom = lastDate.AddDays(-13);
        var previousTo = lastDate.AddDays(-7);

        var current = values
            .Where(x => x.Date >= currentFrom && x.Date <= lastDate)
            .Sum(x => Math.Max(0, x.Value));

        var previous = values
            .Where(x => x.Date >= previousFrom && x.Date <= previousTo)
            .Sum(x => Math.Max(0, x.Value));

        if (previous == 0) return current == 0 ? 0 : 1;
        return Math.Clamp((current - previous) / (double)previous, -1, 5);
    }

    private static double NormalizeGrowth(double growth) =>
        Math.Clamp((growth + 1) / 2.0, 0, 1);

    private static string Get(IReadOnlyDictionary<string, string> row, string name) =>
        row.TryGetValue(name, out var value) ? value.Trim() : string.Empty;

    private static bool EqualsIgnoreCase(
        IReadOnlyDictionary<string, string> row,
        string name,
        string expected) =>
        string.Equals(Get(row, name), expected, StringComparison.OrdinalIgnoreCase);

    private static long ParseLong(IReadOnlyDictionary<string, string> row, string name) =>
        long.TryParse(Get(row, name), NumberStyles.Any, CultureInfo.InvariantCulture, out var value)
            ? value
            : 0;

    private static int? ParseInt(IReadOnlyDictionary<string, string> row, string name) =>
        int.TryParse(Get(row, name), NumberStyles.Integer, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static double? ParseDouble(IReadOnlyDictionary<string, string> row, string name) =>
        double.TryParse(Get(row, name), NumberStyles.Float, CultureInfo.InvariantCulture, out var value)
            ? value
            : null;

    private static string FirstNotBlank(params string?[] values) =>
        values.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x)) ?? "Unknown";

    private sealed record GoogleRegion(
        string Key,
        string Name,
        string? Code,
        int AggregationLevel,
        double? Latitude,
        double? Longitude);

    private sealed record GoogleIndexReadResult(
        int RecordsRead,
        IReadOnlyDictionary<string, GoogleRegion> Regions);

    private sealed record GoogleEpiRecord(
        DateOnly Date,
        string Key,
        long NewCases,
        long NewDeaths,
        long CumulativeCases,
        long CumulativeDeaths);

    private sealed record GoogleEpiReadResult(
        int RecordsRead,
        IReadOnlyList<GoogleEpiRecord> Records);

    private sealed record WhoRecord(
        DateOnly Date,
        long NewCases,
        long NewDeaths,
        long CumulativeCases,
        long CumulativeDeaths);

    private sealed record WhoReadResult(
        int RecordsRead,
        IReadOnlyList<WhoRecord> Records);

    private sealed record RegionalWorking(
        GoogleRegion Region,
        long TotalCases,
        long TotalDeaths,
        long RecentCases,
        long RecentDeaths,
        double CaseGrowth,
        double DeathGrowth);

    private sealed record RegionalSimulation(
        RegionalWorking Working,
        double RiskScore,
        double ExpectedVentilators,
        double ExpectedPpe);

    /// <summary>
    /// Small dependency-free streaming CSV reader supporting quoted commas
    /// and escaped quotes. WHO and Google source tables are record-per-line
    /// CSV files, so line streaming keeps memory usage bounded.
    /// </summary>
    private sealed class CsvStreamReader
    {
        private readonly TextReader _reader;
        private readonly string[] _headers;

        private CsvStreamReader(TextReader reader, string[] headers)
        {
            _reader = reader;
            _headers = headers;
        }

        public static async Task<CsvStreamReader> CreateAsync(
            TextReader reader,
            CancellationToken cancellationToken)
        {
            var headerLine = await ReadLineAsync(reader, cancellationToken)
                ?? throw new InvalidOperationException("CSV source did not contain a header row.");

            var header = ParseLine(headerLine);
            if (header.Count > 0)
                header[0] = header[0].TrimStart('\uFEFF');

            return new CsvStreamReader(reader, header.ToArray());
        }

        public async Task<Dictionary<string, string>?> ReadAsync(
            CancellationToken cancellationToken)
        {
            var line = await ReadLineAsync(_reader, cancellationToken);
            if (line is null) return null;

            var fields = ParseLine(line);
            var row = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            for (var i = 0; i < _headers.Length; i++)
                row[_headers[i]] = i < fields.Count ? fields[i] : string.Empty;

            return row;
        }

        private static async Task<string?> ReadLineAsync(
            TextReader reader,
            CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (reader is StreamReader streamReader)
                return await streamReader.ReadLineAsync(cancellationToken);

            return await reader.ReadLineAsync();
        }

        private static List<string> ParseLine(string line)
        {
            var fields = new List<string>();
            var field = new System.Text.StringBuilder();
            var inQuotes = false;

            for (var i = 0; i < line.Length; i++)
            {
                var ch = line[i];

                if (ch == '"')
                {
                    if (inQuotes && i + 1 < line.Length && line[i + 1] == '"')
                    {
                        field.Append('"');
                        i++;
                    }
                    else
                    {
                        inQuotes = !inQuotes;
                    }

                    continue;
                }

                if (ch == ',' && !inQuotes)
                {
                    fields.Add(field.ToString());
                    field.Clear();
                    continue;
                }

                field.Append(ch);
            }

            fields.Add(field.ToString());
            return fields;
        }
    }
}