# Existing test behavior index: Algocura.Screening.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Screening.Api.Tests/Controllers/ScreeningSynchronizationControllerTests.cs

- L18: `[Fact]`
- L19: `public async Task SynchronizeAllCompleted_PublishesSnapshotForEachCompletedScreening()`
- L99: `[Fact]`
- L100: `public async Task SynchronizeAllCompleted_WhenNoCompletedScreenings_ReturnsEmptyResult()`
- L150: `[Fact]`
- L151: `public async Task SynchronizeById_CompletedScreening_PublishesSnapshot()`
- L212: `[Fact]`
- L213: `public async Task SynchronizeById_UnknownScreening_ReturnsNotFound()`
- L233: `[Fact]`
- L234: `public async Task SynchronizeById_NonCompletedScreening_ReturnsConflict()`
- L290: `[Fact]`
- L291: `public async Task SynchronizeAllCompleted_WhenPublishingFails_ReportsFailure()`
## Algocura.Screening.Api.Tests/Controllers/ScreeningsControllerTests.cs

- L17: `[Fact]`
- L18: `public async Task Create_WithKnownCompounds_CreatesQueuedScreeningAndPublishesEvent()`
- L67: `[Fact]`
- L68: `public async Task Create_WithUnknownCompound_ReturnsValidationProblem()`
- L99: `[Fact]`
- L100: `public async Task Start_QueuedScreening_ChangesStatusAndPublishesEvent()`
- L142: `[Fact]`
- L143: `public async Task Complete_RunningScreening_RanksResultsDescending()`
- L222: `[Fact]`
- L223: `public async Task Delete_RunningScreening_ReturnsConflict()`
## Algocura.Screening.Api.Tests/Data/ScreeningDbContextTests.cs

- L10: `[Fact]`
- L11: `public async Task SaveAndLoad_ScreeningWithCandidatesAndResults_Works()`
## Algocura.Screening.Api.Tests/GlobalUsings.cs

## Algocura.Screening.Api.Tests/Messaging/CompoundEventsConsumerTests.cs

- L10: `[Fact]`
- L11: `public async Task Handle_RegisteredCompound_InsertsLocalProjection()`
- L45: `[Fact]`
- L46: `public async Task Handle_UpdatedCompound_UpdatesExistingProjection()`
## Algocura.Screening.Api.Tests/Support/TestDb.cs

