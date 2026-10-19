# Existing test behavior index: Algocura.ChemicalDesign.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.ChemicalDesign.Api.Tests/Controllers/DesignProjectsControllerTests.cs

- L16: `[Fact]`
- L17: `public async Task Create_KnownTarget_CreatesDraftProject()`
## Algocura.ChemicalDesign.Api.Tests/Controllers/StructureSearchesControllerTests.cs

- L15: `[Fact]`
- L16: `public async Task Run_QueuedExactSearch_CreatesCandidate()`
## Algocura.ChemicalDesign.Api.Tests/GlobalUsings.cs

## Algocura.ChemicalDesign.Api.Tests/Messaging/TargetDiscoveryEventsConsumerTests.cs

- L11: `[Fact]`
- L12: `public async Task Handle_Snapshot_InsertsProjection()`
- L34: `[Fact]`
- L35: `public async Task Handle_Replay_UpdatesWithoutDuplicate()`
## Algocura.ChemicalDesign.Api.Tests/Services/StructureSearchEngineTests.cs

- L9: `[Fact]`
- L10: `public void Search_ExactMatch_ReturnsMatchingEntry()`
- L56: `[Fact]`
- L57: `public void Search_InvalidThreshold_Throws()`
## Algocura.ChemicalDesign.Api.Tests/Support/TestDatabase.cs

