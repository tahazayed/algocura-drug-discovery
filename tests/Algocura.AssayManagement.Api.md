# Existing test behavior index: Algocura.AssayManagement.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.AssayManagement.Api.Tests/Controllers/AssaySynchronizationControllerTests.cs

- L15: `[Fact]`
- L16: `public async Task SynchronizePublishedAssays_PublishesOnlyPublishedAssays()`
- L97: `[Fact]`
- L98: `public async Task SynchronizeById_PublishedAssay_PublishesSnapshot()`
- L168: `[Fact]`
- L169: `public async Task SynchronizeById_CompletedAssay_ReturnsConflict()`
## Algocura.AssayManagement.Api.Tests/Controllers/AssaysControllerTests.cs

- L17: `[Fact]`
- L18: `public async Task Create_KnownScreening_CreatesDraftAssay()`
- L74: `[Fact]`
- L75: `public async Task Complete_AssayWithMeasurements_CreatesResults()`
## Algocura.AssayManagement.Api.Tests/GlobalUsings.cs

## Algocura.AssayManagement.Api.Tests/Messaging/ScreeningEventsConsumerTests.cs

- L11: `[Fact]`
- L12: `public async Task Handle_Snapshot_InsertsScreeningProjection()`
- L49: `[Fact]`
- L50: `public async Task Handle_ReplayedSnapshot_ReplacesResults()`
## Algocura.AssayManagement.Api.Tests/Services/AssayResultCalculatorTests.cs

- L9: `[Fact]`
- L10: `public void Calculate_Replicates_CreatesCompoundResult()`
## Algocura.AssayManagement.Api.Tests/Support/TestDatabase.cs

