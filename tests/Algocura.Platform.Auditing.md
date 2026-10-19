# Existing test behavior index: Algocura.Platform.Auditing

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Platform.Auditing.Tests/GlobalUsings.cs

## Algocura.Platform.Auditing.Tests/Identity/CurrentUserContextTests.cs

- L11: `[Fact]`
- L12: `public void AuthenticatedUser_ReadsClaims()`
## Algocura.Platform.Auditing.Tests/Interceptors/AuditableEntityInterceptorTests.cs

- L14: `[Fact]`
- L15: `public async Task SaveChanges_NewEntity_StampsAuditMetadata()`
- L77: `[Fact]`
- L78: `public async Task SaveChanges_ModifiedEntity_PreservesCreatedMetadata()`
## Algocura.Platform.Auditing.Tests/Interceptors/EntityAuditEventInterceptorTests.cs

- L16: `[Fact]`
- L17: `public async Task SaveChanges_NewEntity_PublishesAuditEvent()`
- L163: `[Fact]`
- L164: `public async Task SaveChanges_ModifiedEntity_PublishesPreviousAndCurrentValues()`
- L281: `[Fact]`
- L282: `public async Task SaveChanges_DeletedEntity_PublishesDeleteAuditEvent()`
## Algocura.Platform.Auditing.Tests/Sanitization/DefaultAuditValueSanitizerTests.cs

- L9: `[Fact]`
- L10: `public void Sanitize_ExcludesSecretsAndMasksEmail()`
## Algocura.Platform.Auditing.Tests/Support/TestDbContext.cs

