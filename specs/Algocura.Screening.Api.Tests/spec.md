# Algocura.Screening.Api.Tests — source-derived rebuild specification

> Status: source-derived technical inventory, not independently validated business requirements. Rebuild for behavioral parity; do not assume missing details.

Files analyzed: 7.

## Project definition (authoritative build metadata)

Source: `Algocura.Screening.Api.Tests/Algocura.Screening.Api.Tests.csproj`
```xml
﻿<Project Sdk="Microsoft.NET.Sdk">

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
			<IncludeAssets>
				runtime;
				build;
				native;
				contentfiles;
				analyzers;
				buildtransitive
			</IncludeAssets>
		</PackageReference>

		<PackageReference Include="FluentAssertions" Version="8.10.0" />

		<PackageReference Include="Microsoft.AspNetCore.Mvc.Testing" Version="10.0.10" />

		<PackageReference Include="Microsoft.EntityFrameworkCore.Sqlite" Version="10.0.10" />

		<PackageReference Include="Microsoft.NET.Test.Sdk" Version="17.14.1" />

		<PackageReference Include="NSubstitute" Version="6.0.0" />

		<PackageReference Include="xunit" Version="2.9.3" />

		<PackageReference Include="xunit.runner.visualstudio" Version="3.1.4">
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
		<PackageReference Include="SQLitePCLRaw.lib.e_sqlite3" Version="2.1.12" />
	</ItemGroup>

	<ItemGroup>
		<ProjectReference Include="..\Algocura.Screening.Api\Algocura.Screening.Api.csproj" />
	</ItemGroup>

</Project>
```

## Source inventory

| Path | Lines |
|---|---:|
| `Algocura.Screening.Api.Tests/Algocura.Screening.Api.Tests.csproj` | 54 |
| `Algocura.Screening.Api.Tests/Controllers/ScreeningSynchronizationControllerTests.cs` | 392 |
| `Algocura.Screening.Api.Tests/Controllers/ScreeningsControllerTests.cs` | 250 |
| `Algocura.Screening.Api.Tests/Data/ScreeningDbContextTests.cs` | 31 |
| `Algocura.Screening.Api.Tests/GlobalUsings.cs` | 1 |
| `Algocura.Screening.Api.Tests/Messaging/CompoundEventsConsumerTests.cs` | 96 |
| `Algocura.Screening.Api.Tests/Support/TestDb.cs` | 24 |

## HTTP contract and controller implementation references

Recreate each route, HTTP verb, request/response DTO, validation, authorization and error handling by inspecting the referenced source. Controller method signatures below are extracted from code; endpoint-to-method mapping must be checked against attributes.

### `Algocura.Screening.Api.Tests/Controllers/ScreeningsControllerTests.cs`

- L18: `public async Task Create_WithKnownCompounds_CreatesQueuedScreeningAndPublishesEvent()`
- L68: `public async Task Create_WithUnknownCompound_ReturnsValidationProblem()`
- L100: `public async Task Start_QueuedScreening_ChangesStatusAndPublishesEvent()`
- L143: `public async Task Complete_RunningScreening_RanksResultsDescending()`
- L223: `public async Task Delete_RunningScreening_ReturnsConflict()`

### `Algocura.Screening.Api.Tests/Controllers/ScreeningSynchronizationControllerTests.cs`

- L19: `public async Task SynchronizeAllCompleted_PublishesSnapshotForEachCompletedScreening()`
- L100: `public async Task SynchronizeAllCompleted_WhenNoCompletedScreenings_ReturnsEmptyResult()`
- L151: `public async Task SynchronizeById_CompletedScreening_PublishesSnapshot()`
- L213: `public async Task SynchronizeById_UnknownScreening_ReturnsNotFound()`
- L234: `public async Task SynchronizeById_NonCompletedScreening_ReturnsConflict()`
- L291: `public async Task SynchronizeAllCompleted_WhenPublishingFails_ReportsFailure()`

## Domain and persistence contracts

Rebuild entity classes, EF Core model mappings, relationships, constraints, indexes, default values, and transaction behavior from these source files.

### `Algocura.Screening.Api.Tests/Data/ScreeningDbContextTests.cs`
Types: `ScreeningDbContextTests`

## Service logic and integration signatures

Preserve existing algorithms, dependency injection, event messages, external HTTP clients, state transitions, and exception paths. Signatures alone do not encode method bodies; original source remains required for exact equivalence.

### `Algocura.Screening.Api.Tests/Messaging/CompoundEventsConsumerTests.cs`
- L11: `public async Task Handle_RegisteredCompound_InsertsLocalProjection()`
- L46: `public async Task Handle_UpdatedCompound_UpdatesExistingProjection()`

## Verification and acceptance criteria

- [ ] All original documented routes are present with identical verbs and authorization requirements.
- [ ] All request/response fields, nullability, enum values, error codes, pagination, and validation match the reference.
- [ ] Database schema, constraints, migrations, and seed data are compatible.
- [ ] All observable state transitions and emitted integration events match.
- [ ] Relevant existing tests are recreated and pass; add missing negative-path and authorization tests.
- [ ] Configuration keys are retained but credentials must be supplied separately, never copied from secrets.
- [ ] Open questions are resolved before treating this as a final approved specification.
