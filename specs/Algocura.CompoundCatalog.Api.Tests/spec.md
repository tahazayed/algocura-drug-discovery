# Algocura.CompoundCatalog.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 5.

## Project definition (authoritative build metadata)

Source: `Algocura.CompoundCatalog.Api.Tests/Algocura.CompoundCatalog.Api.Tests.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

  <PropertyGroup>
    <TargetFramework>net10.0</TargetFramework>
    <ImplicitUsings>enable</ImplicitUsings>
    <Nullable>enable</Nullable>
    <IsPackable>false</IsPackable>
  </PropertyGroup>

  <ItemGroup>
    <PackageReference Include="coverlet.collector" Version="6.0.4" />
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
    <PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.10" />
    <PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4" />
	<PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />
  </ItemGroup>

  <ItemGroup>
    <Using Include="Xunit" />
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Algocura.CompoundCatalog.Api\Algocura.CompoundCatalog.Api.csproj" />
  </ItemGroup>

</Project>
```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.CompoundCatalog.Api.Tests/Algocura.CompoundCatalog.Api.Tests.csproj` | 30 |
| `Algocura.CompoundCatalog.Api.Tests/Controllers/CompoundsControllerTests.cs` | 292 |
| `Algocura.CompoundCatalog.Api.Tests/Data/CompoundCatalogDbContextTests.cs` | 68 |
| `Algocura.CompoundCatalog.Api.Tests/Data/CompoundSeedDataTests.cs` | 43 |
| `Algocura.CompoundCatalog.Api.Tests/Infrastructure/SqliteDatabaseFixture.cs` | 47 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.CompoundCatalog.Api.Tests/Controllers/CompoundsControllerTests.cs`

- L26: `public async Task GetAll_ShouldReturnCompounds()`
- L70: `public async Task GetAll_ShouldFilterCompoundsByStatus()`
- L117: `public async Task GetAll_ShouldFilterCompoundsBySearchText()`
- L163: `public async Task GetById_ShouldReturnCompound_WhenCompoundExists()`
- L203: `public async Task GetById_ShouldReturn404_WhenCompoundDoesNotExist()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.CompoundCatalog.Api.Tests/Data/CompoundCatalogDbContextTests.cs`
Types: `CompoundCatalogDbContextTests`

### `Algocura.CompoundCatalog.Api.Tests/Data/CompoundSeedDataTests.cs`
Types: `CompoundSeedDataTests`

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
