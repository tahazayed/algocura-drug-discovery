# Existing test behavior index: Algocura.ClinicalResearch.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.ClinicalResearch.Api.Tests/Controllers/ClinicalDatasetsControllerTests.cs

- L16: `[Fact]`
- L17: `public async Task Create_RegistersDatasetAndPublishesEvent()`
## Algocura.ClinicalResearch.Api.Tests/Controllers/ImagingResearchControllerTests.cs

- L17: `[Fact]`
- L18: `public async Task Curate_FiltersToCtAndMinimumImageCount()`
- L42: `[Fact]`
- L43: `public async Task EstimateResponse_ReturnsResearchOnlyScore()`
- L58: `[Fact]`
- L59: `public async Task Simulator_ProducesDifferentPngAndShrinksPrototypeRoi()`
- L73: `[Fact]`
- L74: `public async Task GetTciaPatients_returns_discovered_patients()`
## Algocura.ClinicalResearch.Api.Tests/Controllers/PublicHealthResearchControllerTests.cs

- L14: `[Fact]`
- L15: `public async Task AnalyzeCovid19_ReturnsAnalysisAndPublishesEvent()`
## Algocura.ClinicalResearch.Api.Tests/Controllers/ResearchSubjectsControllerTests.cs

- L16: `[Fact]`
- L17: `public async Task Import_UpsertsSubjectsAndPublishesSummaryEvent()`
- L54: `[Fact]`
- L55: `public async Task Import_ReimportSameExternalId_UpdatesInsteadOfDuplicating()`
## Algocura.ClinicalResearch.Api.Tests/GlobalUsings.cs

## Algocura.ClinicalResearch.Api.Tests/Support/TestDatabase.cs

