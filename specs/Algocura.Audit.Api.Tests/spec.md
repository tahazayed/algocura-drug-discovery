# Algocura.Audit.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 5.

## Project definition (authoritative build metadata)

Source: `Algocura.Audit.Api.Tests/Algocura.Audit.Api.Tests.csproj`
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
    <PackageReference Include="FluentAssertions" Version="8.10.0" />
	<PackageReference Include="Microsoft.EntityFrameworkCore.InMemory" Version="10.0.10" />
    <PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />
    <PackageReference Include="NSubstitute" Version="6.0.0" />
    <PackageReference Include="xunit" Version="2.9.3" />
    <PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
      <PrivateAssets>all</PrivateAssets>
    </PackageReference>
  </ItemGroup>

  <ItemGroup>
    <ProjectReference Include="..\Algocura.Audit.Api\Algocura.Audit.Api.csproj" />
  </ItemGroup>

</Project>

```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Audit.Api.Tests/Algocura.Audit.Api.Tests.csproj` | 26 |
| `Algocura.Audit.Api.Tests/Controllers/AuditControllerTests.cs` | 155 |
| `Algocura.Audit.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Audit.Api.Tests/Services/AuditIngestionServiceTests.cs` | 137 |
| `Algocura.Audit.Api.Tests/Support/TestDbContextFactory.cs` | 21 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Audit.Api.Tests/Controllers/AuditControllerTests.cs`

- L12: `public async Task Search_ServiceNameFilter_ReturnsMatchingRecord()`
- L92: `public async Task Summary_ReturnsTotals()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Audit.Api.Tests/Services/AuditIngestionServiceTests.cs`
- L12: `public async Task IngestAsync_NewEvent_StoresAuditRecord()`
- L78: `public async Task IngestAsync_DuplicateEvent_IsIdempotent()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
