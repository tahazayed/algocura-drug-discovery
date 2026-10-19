# Algocura.TargetDiscovery.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.TargetDiscovery.Api.Tests/Algocura.TargetDiscovery.Api.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.TargetDiscovery.Api\Algocura.TargetDiscovery.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.TargetDiscovery.Api.Tests/Algocura.TargetDiscovery.Api.Tests.csproj` | 32 |
| `Algocura.TargetDiscovery.Api.Tests/Controllers/TargetAssessmentsControllerTests.cs` | 255 |
| `Algocura.TargetDiscovery.Api.Tests/Controllers/TargetDiscoverySynchronizationControllerTests.cs` | 136 |
| `Algocura.TargetDiscovery.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.TargetDiscovery.Api.Tests/Messaging/LiteratureIntelligenceEventsConsumerTests.cs` | 131 |
| `Algocura.TargetDiscovery.Api.Tests/Services/TargetAssessmentScoringEngineTests.cs` | 71 |
| `Algocura.TargetDiscovery.Api.Tests/Support/TestDatabase.cs` | 48 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.TargetDiscovery.Api.Tests/Controllers/TargetAssessmentsControllerTests.cs`

- L18: `public async Task Create_KnownHypothesis_CreatesDraftAssessment()`
- L80: `public async Task Complete_DraftAssessment_CalculatesScore()`
- L136: `public async Task Select_CompletedAssessment_PublishesTargetSelected()`
- L210: `private static TargetHypothesisProjection CreateHypothesis()`

### `Algocura.TargetDiscovery.Api.Tests/Controllers/TargetDiscoverySynchronizationControllerTests.cs`

- L16: `public async Task SynchronizeSelectedTargets_PublishesOnlySelectedAssessments()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.TargetDiscovery.Api.Tests/Messaging/LiteratureIntelligenceEventsConsumerTests.cs`
- L12: `public async Task Handle_Snapshot_InsertsHypothesisProjection()`
- L55: `public async Task Handle_ReplayedSnapshot_UpdatesWithoutDuplicates()`

### `Algocura.TargetDiscovery.Api.Tests/Services/TargetAssessmentScoringEngineTests.cs`
- L10: `public void Calculate_StrongTarget_ReturnsHighScore()`
- L44: `public void Calculate_InvalidScore_Throws()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
