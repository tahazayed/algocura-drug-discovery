# Algocura.AIDiscovery.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.AIDiscovery.Api.Tests/Algocura.AIDiscovery.Api.Tests.csproj`
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
    <Compile Include="..\..\..\..\..\tfs\Algocura.DrugDiscovery\Algocura.Screening.Api.Tests\GlobalUsings.cs" Link="GlobalUsings.cs" />
  </ItemGroup>
  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <PrivateAssets>all</PrivateAssets>
      <IncludeAssets>runtime; build; native; contentfiles; analyzers; buildtransitive</IncludeAssets>
    </PackageReference>
	<PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />
  </ItemGroup>
  <ItemGroup>
    <ProjectReference Include="..\Algocura.AIDiscovery.Api\Algocura.AIDiscovery.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.AIDiscovery.Api.Tests/Algocura.AIDiscovery.Api.Tests.csproj` | 32 |
| `Algocura.AIDiscovery.Api.Tests/Controllers/PredictionsControllerTests.cs` | 162 |
| `Algocura.AIDiscovery.Api.Tests/Data/AdaptiveRoutingSeedConfigurationTests.cs` | 56 |
| `Algocura.AIDiscovery.Api.Tests/Messaging/PlatformEventsConsumerTests.cs` | 373 |
| `Algocura.AIDiscovery.Api.Tests/Routing/AdaptiveModelRouterTests.cs` | 86 |
| `Algocura.AIDiscovery.Api.Tests/Services/PredictionScoringTests.cs` | 63 |
| `Algocura.AIDiscovery.Api.Tests/Support/TestDatabase.cs` | 42 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.AIDiscovery.Api.Tests/Controllers/PredictionsControllerTests.cs`

- L19: `public async Task Create_WithKnownCompound_CreatesQueuedPrediction()`
- L76: `public async Task Run_QueuedPrediction_CompletesAndRanksResults()`
- L147: `private static ModelDefinition CreateModel()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.AIDiscovery.Api.Tests/Data/AdaptiveRoutingSeedConfigurationTests.cs`
Types: `AdaptiveRoutingSeedConfigurationTests`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.AIDiscovery.Api.Tests/Messaging/PlatformEventsConsumerTests.cs`
- L14: `public async Task Handle_CompoundRegistered_InsertsProjection()`
- L61: `public async Task Handle_CompoundRegistered_ReplayedEventUpdatesExistingProjection()`
- L126: `public async Task Handle_CompletedScreeningSnapshot_InsertsScreeningProjection()`
- L199: `public async Task Handle_CompletedScreeningSnapshot_ReplayReplacesResultsWithoutDuplicates()`
- L313: `public async Task Handle_CompletedScreeningSnapshot_WithEmptyResults_ClearsExistingResults()`

### `Algocura.AIDiscovery.Api.Tests/Services/PredictionScoringTests.cs`
- L10: `public void Score_HigherScreeningScore_RanksFirst()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
