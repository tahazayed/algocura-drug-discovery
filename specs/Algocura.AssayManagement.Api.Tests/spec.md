# Algocura.AssayManagement.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.AssayManagement.Api.Tests/Algocura.AssayManagement.Api.Tests.csproj`
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
    <ProjectReference Include="..\Algocura.AssayManagement.Api\Algocura.AssayManagement.Api.csproj" />
  </ItemGroup>
</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.AssayManagement.Api.Tests/Algocura.AssayManagement.Api.Tests.csproj` | 31 |
| `Algocura.AssayManagement.Api.Tests/Controllers/AssaySynchronizationControllerTests.cs` | 311 |
| `Algocura.AssayManagement.Api.Tests/Controllers/AssaysControllerTests.cs` | 212 |
| `Algocura.AssayManagement.Api.Tests/GlobalUsings.cs` | 2 |
| `Algocura.AssayManagement.Api.Tests/Messaging/ScreeningEventsConsumerTests.cs` | 124 |
| `Algocura.AssayManagement.Api.Tests/Services/AssayResultCalculatorTests.cs` | 75 |
| `Algocura.AssayManagement.Api.Tests/Support/TestDatabase.cs` | 48 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.AssayManagement.Api.Tests/Controllers/AssaysControllerTests.cs`

- L18: `public async Task Create_KnownScreening_CreatesDraftAssay()`
- L75: `public async Task Complete_AssayWithMeasurements_CreatesResults()`
- L163: `private static CompletedScreeningProjection CreateScreening()`

### `Algocura.AssayManagement.Api.Tests/Controllers/AssaySynchronizationControllerTests.cs`

- L16: `public async Task SynchronizePublishedAssays_PublishesOnlyPublishedAssays()`
- L98: `public async Task SynchronizeById_PublishedAssay_PublishesSnapshot()`
- L169: `public async Task SynchronizeById_CompletedAssay_ReturnsConflict()`
- L221: `private static CompletedScreeningProjection CreateScreening()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.AssayManagement.Api.Tests/Messaging/ScreeningEventsConsumerTests.cs`
- L12: `public async Task Handle_Snapshot_InsertsScreeningProjection()`
- L50: `public async Task Handle_ReplayedSnapshot_ReplacesResults()`

### `Algocura.AssayManagement.Api.Tests/Services/AssayResultCalculatorTests.cs`
- L10: `public void Calculate_Replicates_CreatesCompoundResult()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
