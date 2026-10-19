# Existing test behavior index: Algocura.AIDiscovery.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.AIDiscovery.Api.Tests/Controllers/PredictionsControllerTests.cs

- L18: `[Fact]`
- L19: `public async Task Create_WithKnownCompound_CreatesQueuedPrediction()`
- L75: `[Fact]`
- L76: `public async Task Run_QueuedPrediction_CompletesAndRanksResults()`
## Algocura.AIDiscovery.Api.Tests/Data/AdaptiveRoutingSeedConfigurationTests.cs

- L8: `[Fact]`
- L9: `public void SeededAdaptiveModelIds_AreUnique()`
- L29: `[Fact]`
- L30: `public void SeededAdaptiveModelIds_AreDeterministic()`
## Algocura.AIDiscovery.Api.Tests/Messaging/PlatformEventsConsumerTests.cs

- L13: `[Fact]`
- L14: `public async Task Handle_CompoundRegistered_InsertsProjection()`
- L60: `[Fact]`
- L61: `public async Task Handle_CompoundRegistered_ReplayedEventUpdatesExistingProjection()`
- L125: `[Fact]`
- L126: `public async Task Handle_CompletedScreeningSnapshot_InsertsScreeningProjection()`
- L198: `[Fact]`
- L199: `public async Task Handle_CompletedScreeningSnapshot_ReplayReplacesResultsWithoutDuplicates()`
- L312: `[Fact]`
- L313: `public async Task Handle_CompletedScreeningSnapshot_WithEmptyResults_ClearsExistingResults()`
## Algocura.AIDiscovery.Api.Tests/Routing/AdaptiveModelRouterTests.cs

- L9: `[Fact]`
- L10: `public void Route_SelectsHigherConfidenceSpecialistWhenBudgetAllows()`
- L33: `[Fact]`
- L34: `public void Route_ExcludesPreviouslyExecutedModel()`
## Algocura.AIDiscovery.Api.Tests/Services/PredictionScoringTests.cs

- L9: `[Fact]`
- L10: `public void Score_HigherScreeningScore_RanksFirst()`
## Algocura.AIDiscovery.Api.Tests/Support/TestDatabase.cs

