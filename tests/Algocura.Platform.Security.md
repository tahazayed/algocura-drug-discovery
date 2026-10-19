# Existing test behavior index: Algocura.Platform.Security

These names are evidence of tested behavior, not proof of full coverage.

## Algocura.Platform.Security.Tests/Authorization/PlatformPoliciesTests.cs

- L8: `[Fact]`
- L9: `public void PolicyNames_AreStable()`
## Algocura.Platform.Security.Tests/Authorization/PlatformRolesTests.cs

- L8: `[Fact]`
- L9: `public void All_ContainsExpectedPlatformRoles()`
- L20: `[Fact]`
- L21: `public void RoleNames_AreStable()`
## Algocura.Platform.Security.Tests/Claims/PlatformClaimTypesTests.cs

- L8: `[Fact]`
- L9: `public void ClaimTypeNames_AreStable()`
## Algocura.Platform.Security.Tests/Context/HttpCurrentUserContextTests.cs

- L11: `[Fact]`
- L12: `public void AuthenticatedUser_ExposesCurrentUserMetadata()`
- L45: `[Fact]`
- L46: `public void MissingHttpContext_ReturnsAnonymousValues()`
## Algocura.Platform.Security.Tests/DependencyInjection/SecurityServiceCollectionExtensionsTests.cs

- L19: `[Fact]`
- L20: `public void AddAlgocuraPlatformSecurity_RegistersCoreSecurityServices()`
- L36: `[Fact]`
- L37: `public void AddAlgocuraPlatformSecurity_BindsJwtOptions()`
- L53: `[Fact]`
- L54: `public void AddAlgocuraPlatformSecurity_ConfiguresJwtBearerAsDefaultScheme()`
- L67: `[Theory]`
- L68: `[InlineData(PlatformPolicies.AuthenticatedUser)]`
- L69: `[InlineData(PlatformPolicies.AdministratorOnly)]`
- L70: `[InlineData(PlatformPolicies.ScientistOrAdministrator)]`
- L71: `[InlineData(PlatformPolicies.ReviewerOrAdministrator)]`
- L72: `[InlineData(PlatformPolicies.ServiceOrAdministrator)]`
- L73: `public async Task AddAlgocuraPlatformSecurity_RegistersBuiltInPolicies(string policyName)`
- L87: `[Fact]`
- L88: `public async Task AdministratorOnlyPolicy_RequiresAdministratorRole()`
## Algocura.Platform.Security.Tests/Extensions/ClaimsPrincipalExtensionsTests.cs

- L10: `[Fact]`
- L11: `public void GetUserId_UsesNameIdentifierClaim()`
- L22: `[Fact]`
- L23: `public void GetUserId_FallsBackToSubjectClaim()`
- L34: `[Fact]`
- L35: `public void GetUserId_WithInvalidGuid_ReturnsNull()`
- L45: `[Fact]`
- L46: `public void GetDisplayName_ReturnsNameClaim()`
- L56: `[Fact]`
- L57: `public void GetEmail_UsesEmailClaim()`
- L67: `[Fact]`
- L68: `public void GetRoles_ReturnsDistinctRolesIgnoringCase()`
## Algocura.Platform.Security.Tests/GlobalUsings.cs

## Algocura.Platform.Security.Tests/Options/JwtOptionsTests.cs

- L8: `[Fact]`
- L9: `public void Defaults_AreExpected()`
