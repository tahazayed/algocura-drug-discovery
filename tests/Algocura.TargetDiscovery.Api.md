# Existing test behavior index: Algocura.TargetDiscovery.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.TargetDiscovery.Api.Tests/Controllers/TargetAssessmentsControllerTests.cs

- L17: `[Fact]`
- L18: `public async Task Create_KnownHypothesis_CreatesDraftAssessment()`
- L79: `[Fact]`
- L80: `public async Task Complete_DraftAssessment_CalculatesScore()`
- L135: `[Fact]`
- L136: `public async Task Select_CompletedAssessment_PublishesTargetSelected()`
## Algocura.TargetDiscovery.Api.Tests/Controllers/TargetDiscoverySynchronizationControllerTests.cs

- L15: `[Fact]`
- L16: `public async Task SynchronizeSelectedTargets_PublishesOnlySelectedAssessments()`
## Algocura.TargetDiscovery.Api.Tests/GlobalUsings.cs

## Algocura.TargetDiscovery.Api.Tests/Messaging/LiteratureIntelligenceEventsConsumerTests.cs

- L11: `[Fact]`
- L12: `public async Task Handle_Snapshot_InsertsHypothesisProjection()`
- L54: `[Fact]`
- L55: `public async Task Handle_ReplayedSnapshot_UpdatesWithoutDuplicates()`
## Algocura.TargetDiscovery.Api.Tests/Services/TargetAssessmentScoringEngineTests.cs

- L9: `[Fact]`
- L10: `public void Calculate_StrongTarget_ReturnsHighScore()`
- L43: `[Fact]`
- L44: `public void Calculate_InvalidScore_Throws()`
## Algocura.TargetDiscovery.Api.Tests/Support/TestDatabase.cs

