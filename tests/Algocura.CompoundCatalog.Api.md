# Existing test behavior index: Algocura.CompoundCatalog.Api

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.CompoundCatalog.Api.Tests/Controllers/CompoundsControllerTests.cs

- L25: `[Fact]`
- L26: `public async Task GetAll_ShouldReturnCompounds()`
- L69: `[Fact]`
- L70: `public async Task GetAll_ShouldFilterCompoundsByStatus()`
- L116: `[Fact]`
- L117: `public async Task GetAll_ShouldFilterCompoundsBySearchText()`
- L162: `[Fact]`
- L163: `public async Task GetById_ShouldReturnCompound_WhenCompoundExists()`
- L202: `[Fact]`
- L203: `public async Task GetById_ShouldReturn404_WhenCompoundDoesNotExist()`
## Algocura.CompoundCatalog.Api.Tests/Data/CompoundCatalogDbContextTests.cs

- L13: `public async Task InitializeAsync()`
- L18: `public async Task DisposeAsync()`
- L23: `[Fact]`
- L24: `public async Task SaveChanges_ShouldPersistCompound()`
- L41: `[Fact]`
- L42: `public async Task SaveChanges_ShouldRejectDuplicateInchiKey()`
## Algocura.CompoundCatalog.Api.Tests/Data/CompoundSeedDataTests.cs

- L8: `[Fact]`
- L9: `public void GetCompounds_ShouldReturnMultipleCompounds()`
- L16: `[Fact]`
- L17: `public void GetCompounds_ShouldHaveUniqueIdentifiers()`
- L32: `[Fact]`
- L33: `public void GetCompounds_ShouldContainRequiredChemicalIdentifiers()`
## Algocura.CompoundCatalog.Api.Tests/Infrastructure/SqliteDatabaseFixture.cs

- L29: `public async Task InitializeAsync()`
- L35: `public async Task ResetAsync()`
