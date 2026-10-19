# Algocura.ClinicalResearch.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.ClinicalResearch.Api.Tests/Algocura.ClinicalResearch.Api.Tests.csproj`
```xml
<Project Sdk="Microsoft.NET.Sdk">
  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
    <IsTestProject>true</IsTestProject>
  </PropertyGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4"><PrivateAssets>all</PrivateAssets><IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets></PackageReference>
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4"><PrivateAssets>all</PrivateAssets><IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets></PackageReference>
    <PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />
    <PackageReference Include="SixLabors.ImageSharp" Version="4.1.1" />
  </ItemGroup>
  <ItemGroup><ProjectReference Include="..\Algocura.ClinicalResearch.Api\Algocura.ClinicalResearch.Api.csproj" /></ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.ClinicalResearch.Api.Tests/Algocura.ClinicalResearch.Api.Tests.csproj` | 22 |
| `Algocura.ClinicalResearch.Api.Tests/Controllers/ClinicalDatasetsControllerTests.cs` | 41 |
| `Algocura.ClinicalResearch.Api.Tests/Controllers/ImagingResearchControllerTests.cs` | 100 |
| `Algocura.ClinicalResearch.Api.Tests/Controllers/PublicHealthResearchControllerTests.cs` | 48 |
| `Algocura.ClinicalResearch.Api.Tests/Controllers/ResearchSubjectsControllerTests.cs` | 80 |
| `Algocura.ClinicalResearch.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.ClinicalResearch.Api.Tests/Support/TestDatabase.cs` | 24 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.ClinicalResearch.Api.Tests/Controllers/ClinicalDatasetsControllerTests.cs`

- L17: `public async Task Create_RegistersDatasetAndPublishesEvent()`

### `Algocura.ClinicalResearch.Api.Tests/Controllers/ImagingResearchControllerTests.cs`

- L18: `public async Task Curate_FiltersToCtAndMinimumImageCount()`
- L43: `public async Task EstimateResponse_ReturnsResearchOnlyScore()`
- L59: `public async Task Simulator_ProducesDifferentPngAndShrinksPrototypeRoi()`
- L74: `public async Task GetTciaPatients_returns_discovered_patients()`
- L98: `private static ImagingResearchController Create(ITciaNbiaClient tcia, IEventPublisher publisher) =>`

### `Algocura.ClinicalResearch.Api.Tests/Controllers/PublicHealthResearchControllerTests.cs`

- L15: `public async Task AnalyzeCovid19_ReturnsAnalysisAndPublishesEvent()`

### `Algocura.ClinicalResearch.Api.Tests/Controllers/ResearchSubjectsControllerTests.cs`

- L17: `public async Task Import_UpsertsSubjectsAndPublishesSummaryEvent()`
- L55: `public async Task Import_ReimportSameExternalId_UpdatesInsteadOfDuplicating()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
