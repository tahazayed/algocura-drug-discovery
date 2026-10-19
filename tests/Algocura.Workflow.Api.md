# Existing test behavior index: Algocura.Workflow.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Workflow.Api.Tests/Controllers/WorkflowsControllerTests.cs

- L16: `[Fact]`
- L17: `public async Task Create_ValidRequest_CreatesSevenStages()`
## Algocura.Workflow.Api.Tests/GlobalUsings.cs

## Algocura.Workflow.Api.Tests/Messaging/PlatformWorkflowEventsConsumerTests.cs

- L12: `[Fact]`
- L13: `public async Task Handle_Event_CompletesStageAndStoresReceipt()`
- L69: `[Fact]`
- L70: `public async Task Handle_ReplayedEvent_IsIdempotent()`
## Algocura.Workflow.Api.Tests/Services/WorkflowStateMachineTests.cs

- L9: `[Fact]`
- L10: `public void Start_DraftWorkflow_ActivatesLiteratureStage()`
- L38: `[Fact]`
- L39: `public void CompleteStage_Literature_ActivatesTargetDiscovery()`
## Algocura.Workflow.Api.Tests/Support/TestDatabase.cs

