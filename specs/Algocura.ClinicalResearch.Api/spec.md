# Algocura.ClinicalResearch.Api — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 42.

## Original documented intent

# Algocura Clinical Research API

Prototype service for registering public/de-identified biomedical research datasets and importing patient/research-subject records with provenance.

## Endpoints

- `GET/POST /api/v1/clinical-datasets`
- `GET/PUT/DELETE /api/v1/clinical-datasets/{id}`
- `GET /api/v1/research-subjects?datasetId={id}`
- `POST /api/v1/research-subjects/datasets/{datasetId}/import`

## Integration events

- `ClinicalDatasetRegistered`
- `ClinicalDatasetUpdated`
- `ClinicalDatasetStatusChanged`
- `ClinicalDatasetDeleted`
- `ResearchSubjectsImported`

The import endpoint upserts by `(ClinicalDatasetId, ExternalSubjectId)` and automatically moves a Draft dataset to Ready after a non-empty import.

For this delivery, database startup uses `EnsureCreatedAsync` so the prototype can run without a generated migration. Before production deployment, create an EF Core migration and switch the initializer to `MigrateAsync` to match the other Algocura services.

## Imaging + immunotherapy research prototype

Research-only endpoints added for the Badry demonstration workflow:

- `GET /api/v1/imaging-research/tcia/series?collection=...&patientId=...` — query public TCIA/NBIA series metadata.
- `POST /api/v1/imaging-research/tcia/curate` — retrieve and filter series by modality/body part/minimum image count.
- `GET /api/v1/imaging-research/tcia/series/{seriesInstanceUid}/download` — download the public DICOM series ZIP from TCIA.
- `POST /api/v1/imaging-research/immunotherapy-response/estimate` — deterministic logistic research-score prototype using normalized hypothetical features. Coefficients are illustrative and are not clinically fitted.
- `POST /api/v1/imaging-research/simulate-followup-ct` — accepts a rendered CT PNG/JPEG plus a manually identified lesion ROI and creates a synthetic follow-up visualization by shrinking the ROI according to the research response score.

The score and synthetic image are explicitly non-clinical. They are suitable for software/architecture demonstration only and must not be represented as validated patient predictions.

## Retrospective Public-Health Research (Wael Demonstration)

The Clinical Research API includes a research-only workflow for reproducing the data-integration pattern described in the Wael expert evaluation using registered public datasets:

- `WHO-COVID19-DAILY` — WHO country-level daily COVID-19 cases/deaths.
- `GOOGLE-COVID19-EPI` — Google regional epidemiology keyed by date + geographic key.
- `GOOGLE-COVID19-GEO` — Google geographic index used to resolve regional keys and aggregation levels.

Endpoints:

```text
GET  /api/v1/public-health-research/wael/datasets/validate
POST /api/v1/public-health-research/covid19/analyze
```

The analysis streams source CSV data, filters it to the requested country/date range, fuses Google regional observations with WHO country-level observations by date, calculates transparent retrospective trend/risk signals, and runs a deterministic-seed Monte Carlo resource-demand simulation with user-visible coefficients.

The resource model is illustrative and not scientifically/clinically validated. It must not be used for real healthcare-resource allocation or patient/public-health decisions.


## Project definition (authoritative build metadata)

Source: `Algocura.ClinicalResearch.Api/Algocura.ClinicalResearch.Api.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk.Web">

	<PropertyGroup>
		<TargetFramework>net10.0</TargetFramework>
		<Nullable>enable</Nullable>
		<ImplicitUsings>enable</ImplicitUsings>
	</PropertyGroup>

	<ItemGroup>
		<PackageReference Include="Azure.Identity" Version="1.*" />
		<PackageReference Include="Azure.Messaging.ServiceBus" Version="7.*" />

		<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="10.0.10" />
		<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="10.0.10" />

		<PackageReference Include="Microsoft.EntityFrameworkCore.Design" Version="10.0.10">
			<PrivateAssets>all</PrivateAssets>
			<IncludeAssets>
				runtime;
				build;
				native;
				contentfiles;
				analyzers;
				buildtransitive
			</IncludeAssets>
		</PackageReference>

		<PackageReference Include="Microsoft.OpenApi" Version="2.7.5" />

		<PackageReference Include="Swashbuckle.AspNetCore.ReDoc" Version="10.2.3" />
		<PackageReference Include="Swashbuckle.AspNetCore.SwaggerUI" Version="10.2.3" />

		<!-- Clinical Research-specific image processing dependency. -->
		<PackageReference Include="SkiaSharp" Version="4.151.2" />
		<PackageReference Include="SkiaSharp.NativeAssets.Linux.NoDependencies" Version="4.151.2" />
	</ItemGroup>
	<ItemGroup>
		<Folder Include="Data\Migrations\" />
	</ItemGroup>
	<ItemGroup>
		<ProjectReference Include="..\Algocura.Platform.Auditing\Algocura.Platform.Auditing.csproj" />
		<ProjectReference Include="..\Algocura.Platform.Messaging\Algocura.Platform.Messaging.csproj" />
		<ProjectReference Include="..\Algocura.Platform.Security\Algocura.Platform.Security.csproj" />
	</ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.ClinicalResearch.Api/Algocura.ClinicalResearch.Api.csproj` | 45 |
| `Algocura.ClinicalResearch.Api/Contracts/ClinicalDatasetContracts.cs` | 63 |
| `Algocura.ClinicalResearch.Api/Contracts/ImagingResearchContracts.cs` | 60 |
| `Algocura.ClinicalResearch.Api/Contracts/PublicHealthResearchContracts.cs` | 120 |
| `Algocura.ClinicalResearch.Api/Contracts/ResearchSubjectContracts.cs` | 65 |
| `Algocura.ClinicalResearch.Api/Controllers/ClinicalDatasetsController.cs` | 291 |
| `Algocura.ClinicalResearch.Api/Controllers/ImagingResearchController.cs` | 126 |
| `Algocura.ClinicalResearch.Api/Controllers/PublicHealthResearchController.cs` | 70 |
| `Algocura.ClinicalResearch.Api/Controllers/ResearchSubjectsController.cs` | 127 |
| `Algocura.ClinicalResearch.Api/Data/ClinicalResearchDbContext.cs` | 57 |
| `Algocura.ClinicalResearch.Api/Data/DatabaseInitializer.cs` | 46 |
| `Algocura.ClinicalResearch.Api/Data/Migrations/20260221015647_InitialClinicalResearch.Designer.cs` | 187 |
| `Algocura.ClinicalResearch.Api/Data/Migrations/20260221015647_InitialClinicalResearch.cs` | 111 |
| `Algocura.ClinicalResearch.Api/Data/Migrations/ClinicalResearchDbContextModelSnapshot.cs` | 184 |
| `Algocura.ClinicalResearch.Api/Domain/ClinicalDataset.cs` | 36 |
| `Algocura.ClinicalResearch.Api/Domain/ClinicalDatasetStatus.cs` | 8 |
| `Algocura.ClinicalResearch.Api/Domain/ResearchSubject.cs` | 47 |
| `Algocura.ClinicalResearch.Api/Events/ClinicalDatasetDeleted.cs` | 7 |
| `Algocura.ClinicalResearch.Api/Events/ClinicalDatasetRegistered.cs` | 14 |
| `Algocura.ClinicalResearch.Api/Events/ClinicalDatasetStatusChanged.cs` | 10 |
| `Algocura.ClinicalResearch.Api/Events/ClinicalDatasetUpdated.cs` | 13 |
| `Algocura.ClinicalResearch.Api/Events/Covid19PublicHealthAnalysisCompleted.cs` | 11 |
| `Algocura.ClinicalResearch.Api/Events/FollowUpCtSimulated.cs` | 9 |
| `Algocura.ClinicalResearch.Api/Events/ImagingSeriesRetrieved.cs` | 9 |
| `Algocura.ClinicalResearch.Api/Events/ImmunotherapyResponseEstimated.cs` | 9 |
| `Algocura.ClinicalResearch.Api/Events/IntegrationEvent.cs` | 12 |
| `Algocura.ClinicalResearch.Api/Events/ResearchSubjectsImported.cs` | 12 |
| `Algocura.ClinicalResearch.Api/Mapping/ClinicalResearchMappings.cs` | 18 |
| `Algocura.ClinicalResearch.Api/Options/TciaNbiaOptions.cs` | 9 |
| `Algocura.ClinicalResearch.Api/Program.cs` | 461 |
| `Algocura.ClinicalResearch.Api/Properties/launchSettings.json` | 12 |
| `Algocura.ClinicalResearch.Api/README.md` | 53 |
| `Algocura.ClinicalResearch.Api/Samples/research-subject-import.sample.json` | 28 |
| `Algocura.ClinicalResearch.Api/Services/IFollowUpCtSimulator.cs` | 8 |
| `Algocura.ClinicalResearch.Api/Services/IImmunotherapyResponseEstimator.cs` | 8 |
| `Algocura.ClinicalResearch.Api/Services/ITciaNbiaClient.cs` | 10 |
| `Algocura.ClinicalResearch.Api/Services/IWaelCovidResearchService.cs` | 13 |
| `Algocura.ClinicalResearch.Api/Services/PrototypeFollowUpCtSimulator.cs` | 324 |
| `Algocura.ClinicalResearch.Api/Services/PrototypeImmunotherapyResponseEstimator.cs` | 32 |
| `Algocura.ClinicalResearch.Api/Services/TciaNbiaClient.cs` | 94 |
| `Algocura.ClinicalResearch.Api/Services/WaelCovidResearchService.cs` | 841 |
| `Algocura.ClinicalResearch.Api/appsettings.json` | 48 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.ClinicalResearch.Api/Controllers/ClinicalDatasetsController.cs`

- L13: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L15: `[Route("api/v1/clinical-datasets")]`
- L22: `[HttpGet]`
- L74: `[HttpGet("{id:guid}")]`
- L104: `[HttpPost]`
- L164: `[HttpPut("{id:guid}")]`
- L249: `[HttpDelete("{id:guid}")]`

### `Algocura.ClinicalResearch.Api/Controllers/ImagingResearchController.cs`

- L11: `[Route("api/v1/imaging-research")]`
- L12: `[Authorize]`
- L20: `[HttpGet("tcia/patients")]`
- L37: `[HttpGet("tcia/series")]`
- L48: `[HttpPost("tcia/curate")]`
- L70: `[HttpGet("tcia/series/{seriesInstanceUid}/download")]`
- L71: `public async Task<IActionResult> DownloadTciaSeries(string seriesInstanceUid, CancellationToken cancellationToken)`
- L77: `[HttpPost("immunotherapy-response/estimate")]`
- L93: `[HttpPost("simulate-followup-ct")]`
- L125: `private static string Sanitize(string value) => string.Concat(value.Select(ch => char.IsLetterOrDigit(ch) || ch is '.' or '-' or '_' ? ch : '_'));`

### `Algocura.ClinicalResearch.Api/Controllers/PublicHealthResearchController.cs`

- L11: `[Route("api/v1/public-health-research")]`
- L12: `[Authorize]`
- L17: `[HttpGet("wael/datasets/validate")]`
- L18: `[ProducesResponseType<WaelDatasetValidationResponse>(StatusCodes.Status200OK)]`
- L25: `[HttpPost("covid19/analyze")]`
- L26: `[ProducesResponseType<Covid19PublicHealthAnalysisResponse>(StatusCodes.Status200OK)]`
- L27: `[ProducesResponseType(StatusCodes.Status400BadRequest)]`

### `Algocura.ClinicalResearch.Api/Controllers/ResearchSubjectsController.cs`

- L13: `[Authorize(Roles = "Scientist,Reviewer,Administrator")]`
- L15: `[Route("api/v1/research-subjects")]`
- L22: `[HttpGet]`
- L52: `[HttpPost("datasets/{datasetId:guid}/import")]`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.ClinicalResearch.Api/Contracts/ClinicalDatasetContracts.cs`
Types: `ClinicalDatasetResponse`, `CreateClinicalDatasetRequest`, `UpdateClinicalDatasetRequest`
- L28: `public string? SourceUri { get; init; }`
- L31: `public string? StudyCode { get; init; }`
- L34: `public string? DiseaseArea { get; init; }`
- L37: `public string? Description { get; init; }`
- L39: `public ClinicalDatasetStatus Status { get; init; } = ClinicalDatasetStatus.Draft;`
- L51: `public string? SourceUri { get; init; }`
- L54: `public string? StudyCode { get; init; }`
- L57: `public string? DiseaseArea { get; init; }`
- L60: `public string? Description { get; init; }`
- L62: `public ClinicalDatasetStatus Status { get; init; }`

### `Algocura.ClinicalResearch.Api/Contracts/ImagingResearchContracts.cs`
Types: `TciaPatientResponse`, `TciaSeriesResponse`, `CuratedImagingSeriesRequest`, `CuratedImagingSeriesResponse`, `ImmunotherapyResponseEstimateRequest`, `ImmunotherapyResponseEstimateResponse`, `SimulatedFollowUpCtResponse`
- L24: `[StringLength(80)] public string? Modality { get; init; } = "CT";`
- L25: `[StringLength(200)] public string? BodyPartContains { get; init; }`
- L26: `public int MinimumImageCount { get; init; } = 1;`
- L36: `[Range(0, 130)] public int Age { get; init; }`
- L37: `[Range(0, 1)] public double NormalizedTumorBurden { get; init; }`
- L38: `[Range(0, 1)] public double NormalizedPdL1 { get; init; }`
- L39: `[Range(0, 1)] public double NormalizedTmb { get; init; }`
- L40: `[Range(0, 1)] public double NormalizedImmuneInfiltration { get; init; }`
- L41: `[Range(0, 1)] public double PriorTreatmentBurden { get; init; }`
- L42: `[StringLength(120)] public string? CancerType { get; init; }`
- L43: `[StringLength(200)] public string? SeriesInstanceUid { get; init; }`

### `Algocura.ClinicalResearch.Api/Contracts/PublicHealthResearchContracts.cs`
Types: `WaelDatasetValidationItem`, `WaelDatasetValidationResponse`, `Covid19PublicHealthAnalysisRequest`, `Covid19DatasetUsage`, `Covid19CountryContext`, `Covid19RegionalSignal`, `Covid19PublicHealthAnalysisResponse`, `Covid19SimulationParameters`
- L25: `public string CountryCode { get; init; } = "US";`
- L28: `public DateOnly StartDate { get; init; } = new(2021, 1, 1);`
- L31: `public DateOnly EndDate { get; init; } = new(2021, 3, 31);`
- L34: `public int GoogleAggregationLevel { get; init; } = 1;`
- L37: `public int MaximumRegions { get; init; } = 60;`
- L40: `public int AvailableVentilators { get; init; } = 500;`
- L43: `public int AvailablePpeUnits { get; init; } = 100_000;`
- L46: `public int SimulationIterations { get; init; } = 250;`
- L49: `public double VentilatorDemandPerCase { get; init; } = 0.02;`
- L52: `public double VentilatorDemandPerDeath { get; init; } = 0.5;`
- L55: `public double PpeUnitsPerCase { get; init; } = 10;`
- L58: `public double PpeUnitsPerDeath { get; init; } = 50;`

### `Algocura.ClinicalResearch.Api/Contracts/ResearchSubjectContracts.cs`
Types: `ResearchSubjectResponse`, `ImportResearchSubjectsRequest`, `ResearchSubjectImportItem`, `ImportResearchSubjectsResponse`
- L33: `public int? AgeAtDiagnosis { get; init; }`
- L36: `public string? Sex { get; init; }`
- L39: `public string? PrimarySite { get; init; }`
- L42: `public string? DiseaseStage { get; init; }`
- L45: `public string? VitalStatus { get; init; }`
- L48: `public string? Diagnosis { get; init; }`
- L51: `public string? TreatmentSummary { get; init; }`
- L54: `public string? MolecularFeaturesJson { get; init; }`
- L57: `public string? AdditionalAttributesJson { get; init; }`

### `Algocura.ClinicalResearch.Api/Data/ClinicalResearchDbContext.cs`
Types: `ClinicalResearchDbContext`
- L9: `public DbSet<ClinicalDataset> ClinicalDatasets => Set<ClinicalDataset>();`
- L10: `public DbSet<ResearchSubject> ResearchSubjects => Set<ResearchSubject>();`
- L16: `var dataset = modelBuilder.Entity<ClinicalDataset>();`
- L32: `var subject = modelBuilder.Entity<ResearchSubject>();`

### `Algocura.ClinicalResearch.Api/Data/DatabaseInitializer.cs`
Types: `DatabaseInitializer`

### `Algocura.ClinicalResearch.Api/Data/Migrations/20260221015647_InitialClinicalResearch.Designer.cs`
Types: `InitialClinicalResearch`
- L28: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ClinicalDataset", b =>`
- L90: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ResearchSubject", b =>`
- L169: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ResearchSubject", b =>`
- L180: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ClinicalDataset", b =>`

### `Algocura.ClinicalResearch.Api/Data/Migrations/20260221015647_InitialClinicalResearch.cs`
Types: `InitialClinicalResearch`

### `Algocura.ClinicalResearch.Api/Data/Migrations/ClinicalResearchDbContextModelSnapshot.cs`
Types: `ClinicalResearchDbContextModelSnapshot`
- L25: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ClinicalDataset", b =>`
- L87: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ResearchSubject", b =>`
- L166: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ResearchSubject", b =>`
- L177: `modelBuilder.Entity("Algocura.ClinicalResearch.Api.Domain.ClinicalDataset", b =>`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.ClinicalResearch.Api/Program.cs`
- L22 registration: `builder.Services.AddHealthChecks();`
- L35 registration: `builder.Services.AddCors(`
- L103 registration: `builder.Services.AddDbContext<ClinicalResearchDbContext>(`
- L161 registration: `builder.Services.AddSingleton<`
- L165 registration: `builder.Services.AddSingleton<`
- L179 registration: `builder.Services.AddScoped<`
- L282 registration: `app.UseAuthentication();`
- L283 registration: `app.UseAuthorization();`
- L348 registration: `app.MapControllers();`

### `Algocura.ClinicalResearch.Api/Services/IFollowUpCtSimulator.cs`

### `Algocura.ClinicalResearch.Api/Services/IImmunotherapyResponseEstimator.cs`

### `Algocura.ClinicalResearch.Api/Services/ITciaNbiaClient.cs`

### `Algocura.ClinicalResearch.Api/Services/IWaelCovidResearchService.cs`

### `Algocura.ClinicalResearch.Api/Services/PrototypeFollowUpCtSimulator.cs`

### `Algocura.ClinicalResearch.Api/Services/PrototypeImmunotherapyResponseEstimator.cs`
- L7: `public ImmunotherapyResponseEstimateResponse Estimate(ImmunotherapyResponseEstimateRequest request)`

### `Algocura.ClinicalResearch.Api/Services/TciaNbiaClient.cs`
- L10: `public sealed class TciaNbiaClient(HttpClient httpClient, IOptions<TciaNbiaOptions> options) : ITciaNbiaClient`
- L42: `public async Task<IReadOnlyCollection<TciaSeriesResponse>> GetSeriesAsync(string collection, string patientId, CancellationToken cancellationToken)`
- L58: `public async Task<byte[]> DownloadSeriesZipAsync(string seriesInstanceUid, CancellationToken cancellationToken)`

### `Algocura.ClinicalResearch.Api/Services/WaelCovidResearchService.cs`
- L303: `private static void ValidateRequest(Covid19PublicHealthAnalysisRequest request)`
- L460: `private static Covid19CountryContext BuildWhoContext(string countryCode, IReadOnlyList<WhoRecord> records)`
- L613: `private static double NextStandardNormal(Random random)`
- L620: `private static int StableHash(string value)`
- L631: `private static int ProportionalAllocation(int available, double demand, double totalDemand)`
- L637: `private static long SumRecent((DateOnly Date, long Value)[] values, int days)`
- L645: `private static double CalculateSevenDayGrowth((DateOnly Date, long Value)[] values)`
- L665: `private static double NormalizeGrowth(double growth) =>`
- L668: `private static string Get(IReadOnlyDictionary<string, string> row, string name) =>`
- L677: `private static long ParseLong(IReadOnlyDictionary<string, string> row, string name) =>`
- L682: `private static int? ParseInt(IReadOnlyDictionary<string, string> row, string name) =>`
- L687: `private static double? ParseDouble(IReadOnlyDictionary<string, string> row, string name) =>`
- L692: `private static string FirstNotBlank(params string?[] values) =>`
- L802: `private static List<string> ParseLine(string line)`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
