# Algocura.ChemicalDesign.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.ChemicalDesign.Api.Tests/Algocura.ChemicalDesign.Api.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.ChemicalDesign.Api\Algocura.ChemicalDesign.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.ChemicalDesign.Api.Tests/Algocura.ChemicalDesign.Api.Tests.csproj` | 29 |
| `Algocura.ChemicalDesign.Api.Tests/Controllers/DesignProjectsControllerTests.cs` | 69 |
| `Algocura.ChemicalDesign.Api.Tests/Controllers/StructureSearchesControllerTests.cs` | 112 |
| `Algocura.ChemicalDesign.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.ChemicalDesign.Api.Tests/Messaging/TargetDiscoveryEventsConsumerTests.cs` | 83 |
| `Algocura.ChemicalDesign.Api.Tests/Services/StructureSearchEngineTests.cs` | 78 |
| `Algocura.ChemicalDesign.Api.Tests/Support/TestDatabase.cs` | 41 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.ChemicalDesign.Api.Tests/Controllers/DesignProjectsControllerTests.cs`

- L17: `public async Task Create_KnownTarget_CreatesDraftProject()`

### `Algocura.ChemicalDesign.Api.Tests/Controllers/StructureSearchesControllerTests.cs`

- L16: `public async Task Run_QueuedExactSearch_CreatesCandidate()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.ChemicalDesign.Api.Tests/Messaging/TargetDiscoveryEventsConsumerTests.cs`
- L12: `public async Task Handle_Snapshot_InsertsProjection()`
- L35: `public async Task Handle_Replay_UpdatesWithoutDuplicate()`

### `Algocura.ChemicalDesign.Api.Tests/Services/StructureSearchEngineTests.cs`
- L10: `public void Search_ExactMatch_ReturnsMatchingEntry()`
- L57: `public void Search_InvalidThreshold_Throws()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
